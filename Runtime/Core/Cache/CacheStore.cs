using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PatchManager.Core.Cache.Json;
using PatchManager.Shared;

namespace PatchManager.Core.Cache
{
    /// <summary>
    /// The patch cache on disk: archives and prefab plans shared by every feature set, and one inventory per set
    /// naming the archives and plans that set uses.
    /// </summary>
    /// <remarks>
    /// Archives are named by their contents, so two sets whose label patches out identical share one file.
    /// </remarks>
    internal class CacheStore : IDisposable
    {
        private const string INVENTORY_FILENAME = "inventory.json";

        private readonly Dictionary<string, Archive> _openArchives = new();

        private Inventory _inventory;

        /// <summary>
        /// Opens the cache rooted at a directory, creating nothing until something is written.
        /// </summary>
        /// <param name="root">The cache's root directory.</param>
        public CacheStore(string root)
        {
            Root = root;
        }

        /// <summary>
        /// Gets the cache's root directory.
        /// </summary>
        public string Root { get; }

        /// <summary>
        /// Gets the directory holding the patched-asset archives every set shares.
        /// </summary>
        public string ArchivesDirectory => Path.Combine(Root, "archives");

        /// <summary>
        /// Gets the directory holding the prefab patch plans every set shares.
        /// </summary>
        public string PlansDirectory => Path.Combine(Root, "prefabs");

        /// <summary>
        /// Gets the directory holding one directory per set.
        /// </summary>
        public string SetsDirectory => Path.Combine(Root, "sets");

        /// <summary>
        /// Gets the id of the set whose inventory is open.
        /// </summary>
        public string ActiveSet { get; private set; }

        /// <summary>
        /// Gets the open set's inventory, loaded from disk on first access.
        /// </summary>
        public Inventory Inventory => _inventory ??= Inventory.Load(InventoryPath(ActiveSet));

        /// <summary>
        /// Opens a set's inventory and marks the set as used now.
        /// </summary>
        /// <remarks>
        /// The mark reaches disk with the next <see cref="SaveInventory" />.
        /// </remarks>
        /// <param name="setId">The set's id, which names its directory.</param>
        public void UseSet(string setId)
        {
            ActiveSet = setId;
            _inventory = null;
            Inventory.LastUsedTicks = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Deletes the open set's inventory and starts it afresh, leaving every other set and every archive alone.
        /// </summary>
        public void InvalidateSet()
        {
            try
            {
                DeleteDirectory(SetDirectory(ActiveSet));
            }
            catch (Exception e)
            {
                Logging.LogError($"Failed to clear the patch cache of set {ActiveSet}: {e}");
            }

            _inventory = Inventory.Create();
            _inventory.LastUsedTicks = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Writes the open set's inventory to disk.
        /// </summary>
        public void SaveInventory()
        {
            Inventory.Save(InventoryPath(ActiveSet));
        }

        /// <summary>
        /// Stores a label's patched assets, unless an archive with exactly these contents is already stored.
        /// </summary>
        /// <param name="entries">The archive's entries as name and text pairs.</param>
        /// <returns>The archive's filename, relative to <see cref="ArchivesDirectory" />.</returns>
        public string WriteArchive(IReadOnlyCollection<(string name, string text)> entries)
        {
            var filename = CacheManager.ContentAddressedName(entries);
            var path = Path.Combine(ArchivesDirectory, filename);
            if (File.Exists(path))
                return filename;

            Directory.CreateDirectory(ArchivesDirectory);

            // Written aside and moved into place, so an interrupted write never leaves a truncated archive under
            // a name that claims its contents
            var tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
            try
            {
                using (var archive = new Archive(tempPath, true))
                {
                    foreach (var (name, text) in entries)
                    {
                        archive.AddFile(name, text);
                    }

                    archive.Save();
                }

                if (!File.Exists(path))
                {
                    File.Move(tempPath, path);
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }

            return filename;
        }

        /// <summary>
        /// Opens a stored archive, or returns the copy already open.
        /// </summary>
        /// <param name="archiveFilename">The archive's filename, relative to <see cref="ArchivesDirectory" />.</param>
        /// <returns>The archive.</returns>
        /// <exception cref="FileNotFoundException">Thrown when no archive by that name is stored.</exception>
        public Archive GetArchive(string archiveFilename)
        {
            if (_openArchives.TryGetValue(archiveFilename, out var open))
                return open;

            var path = Path.Combine(ArchivesDirectory, archiveFilename);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Archive '{path}' does not exist!");

            var archive = new Archive(path);
            _openArchives.Add(archiveFilename, archive);
            return archive;
        }

        /// <summary>
        /// Deletes the least recently used sets above a limit, then every archive and plan no remaining set uses.
        /// </summary>
        /// <remarks>
        /// The open set is never evicted. Files directly under the root are left by the single-inventory layout
        /// that came before sets, and are deleted too.
        /// </remarks>
        /// <param name="maxSets">How many sets may stay.</param>
        /// <param name="pinned">The ids of sets kept whatever their age.</param>
        public void Evict(int maxSets, ISet<string> pinned)
        {
            try
            {
                var inventories = new Dictionary<string, Inventory>();
                if (Directory.Exists(SetsDirectory))
                {
                    foreach (var directory in Directory.GetDirectories(SetsDirectory))
                    {
                        var id = Path.GetFileName(directory);
                        inventories[id] = id == ActiveSet ? Inventory : Inventory.Load(InventoryPath(id));
                    }
                }

                if (ActiveSet != null)
                {
                    inventories[ActiveSet] = Inventory;
                }

                var uses = inventories.Select(
                    pair => new CacheManager.SetUse(
                        pair.Key,
                        pair.Value.LastUsedTicks,
                        pair.Key == ActiveSet || pinned.Contains(pair.Key)
                    )
                );
                foreach (var id in CacheManager.ChooseEvictions(uses, maxSets))
                {
                    Logging.LogDebug($"Evicting the patch cache of set {id}");
                    DeleteDirectory(SetDirectory(id));
                    inventories.Remove(id);
                }

                var archives = new HashSet<string>(
                    inventories.Values.SelectMany(inventory => inventory.CacheEntries.Values)
                        .Select(entry => entry.ArchiveFilename),
                    StringComparer.OrdinalIgnoreCase
                );
                var plans = new HashSet<string>(
                    inventories.Values.SelectMany(inventory => inventory.PlanFiles),
                    StringComparer.OrdinalIgnoreCase
                );
                DeleteUnreferenced(ArchivesDirectory, name => archives.Contains(name));
                // A plan's backup goes with the plan
                DeleteUnreferenced(PlansDirectory, name => plans.Contains(name) || plans.Contains(StripBackup(name)));

                if (Directory.Exists(Root))
                {
                    foreach (var file in Directory.GetFiles(Root))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogError($"Failed to evict stale patch caches: {e}");
            }
        }

        /// <summary>
        /// Closes every open archive.
        /// </summary>
        public void Dispose()
        {
            foreach (var archive in _openArchives.Values)
            {
                archive.Dispose();
            }

            _openArchives.Clear();
        }

        /// <summary>
        /// Gets the directory a set's inventory and summary live in.
        /// </summary>
        /// <param name="setId">The set's id.</param>
        /// <returns>The directory path.</returns>
        public string SetDirectory(string setId) => Path.Combine(SetsDirectory, setId);

        private string InventoryPath(string setId) => Path.Combine(SetDirectory(setId), INVENTORY_FILENAME);

        private static string StripBackup(string name) =>
            name.EndsWith(".bak", StringComparison.OrdinalIgnoreCase) ? name.Substring(0, name.Length - 4) : name;

        private void DeleteUnreferenced(string directory, Func<string, bool> isReferenced)
        {
            if (!Directory.Exists(directory))
                return;

            foreach (var file in Directory.GetFiles(directory))
            {
                var name = Path.GetFileName(file);
                if (isReferenced(name))
                    continue;

                if (_openArchives.TryGetValue(name, out var archive))
                {
                    archive.Dispose();
                    _openArchives.Remove(name);
                }

                File.Delete(file);
            }
        }

        private static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}
