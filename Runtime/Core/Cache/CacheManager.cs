using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PatchManager.Core.Cache.Json;
using PatchManager.PrefabPatching;
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
    internal static class CacheManager
    {
        private const string DEFAULT_ROOT = "./pm_cache";

        private const string INVENTORY_FILENAME = "inventory.json";

        // Set caches kept beyond the pinned ones, least recently used evicted first
        private const int MAX_SETS = 10;

        /// <summary>
        /// Labels whose cached archives are still valid for the current run. Consulted by the addressables resource
        /// locators to decide whether to serve patched assets from the cache.
        /// </summary>
        public static readonly List<string> CacheValidLabels = new();

        private static readonly Dictionary<string, Archive> OpenArchives = new();

        private static Inventory _inventory;

        /// <summary>
        /// The cache's root directory. Tests point it at a directory of their own.
        /// </summary>
        internal static string Root = DEFAULT_ROOT;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Close();
            Root = DEFAULT_ROOT;
        }

        /// <summary>
        /// Closes every open archive and forgets the open set, so the next use reads the cache from disk afresh.
        /// </summary>
        internal static void Close()
        {
            foreach (var archive in OpenArchives.Values)
            {
                archive.Dispose();
            }

            OpenArchives.Clear();
            CacheValidLabels.Clear();
            _inventory = null;
            ActiveSet = null;
        }

        /// <summary>
        /// Gets the directory holding the patched-asset archives every set shares.
        /// </summary>
        public static string ArchivesDirectory => Path.Combine(Root, "archives");

        /// <summary>
        /// Gets the directory holding the prefab patch plans every set shares.
        /// </summary>
        public static string PlansDirectory => Path.Combine(Root, "prefabs");

        /// <summary>
        /// Gets the directory holding one directory per set.
        /// </summary>
        public static string SetsDirectory => Path.Combine(Root, "sets");

        /// <summary>
        /// Gets the id of the set whose inventory is open.
        /// </summary>
        public static string ActiveSet { get; private set; }

        /// <summary>
        /// Gets the open set's inventory, loaded from disk on first access.
        /// </summary>
        public static Inventory Inventory => _inventory ??= Inventory.Load(InventoryPath(ActiveSet));

        /// <summary>
        /// Opens a set's inventory and marks the set as used now.
        /// </summary>
        /// <remarks>
        /// The mark reaches disk with the next <see cref="SaveInventory" />.
        /// </remarks>
        /// <param name="setId">The set's id, which names its directory.</param>
        public static void UseSet(string setId)
        {
            CacheValidLabels.Clear();
            ActiveSet = setId;
            _inventory = null;
            Inventory.LastUsedTicks = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Discards the open set's cache and starts it afresh, leaving every other set and every archive alone.
        /// </summary>
        public static void InvalidateCache()
        {
            CacheValidLabels.Clear();
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
        public static void SaveInventory()
        {
            Inventory.Save(InventoryPath(ActiveSet));
        }

        /// <summary>
        /// Stores a label's patched assets, unless an archive with exactly these contents is already stored.
        /// </summary>
        /// <param name="entries">The archive's entries as name and text pairs.</param>
        /// <returns>The archive's filename, relative to <see cref="ArchivesDirectory" />.</returns>
        public static string WriteArchive(IReadOnlyCollection<(string name, string text)> entries)
        {
            var filename = ContentAddressedName(entries);
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
        public static Archive GetArchive(string archiveFilename)
        {
            if (OpenArchives.TryGetValue(archiveFilename, out var open))
                return open;

            var path = Path.Combine(ArchivesDirectory, archiveFilename);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Archive '{path}' does not exist!");

            var archive = new Archive(path);
            OpenArchives.Add(archiveFilename, archive);
            return archive;
        }

        /// <summary>
        /// Deletes the least recently used set caches above the limit, then every archive and plan no remaining set
        /// uses.
        /// </summary>
        /// <remarks>
        /// The open set is never evicted. Files directly under the root are left by the single-inventory layout
        /// that came before sets, and are deleted too.
        /// </remarks>
        /// <param name="pinned">The ids of sets kept whatever their age, such as those packs and saves use.</param>
        public static void EvictStaleSets(ISet<string> pinned)
        {
            EvictStaleSets(MAX_SETS, pinned);
        }

        /// <summary>
        /// Deletes the least recently used set caches above a limit, then every archive and plan no remaining set
        /// uses.
        /// </summary>
        /// <param name="maxSets">How many sets may stay.</param>
        /// <param name="pinned">The ids of sets kept whatever their age.</param>
        internal static void EvictStaleSets(int maxSets, ISet<string> pinned)
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
                    pair => (pair.Key, pair.Value.LastUsedTicks, pair.Key == ActiveSet || pinned.Contains(pair.Key))
                );
                foreach (var id in ChooseEvictions(uses, maxSets))
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
        /// Picks the feature set caches to delete so that at most <paramref name="maxSets" /> unpinned ones remain,
        /// least recently used first.
        /// </summary>
        /// <param name="sets">Every set on disk, with when it was last used in UTC ticks and whether it is kept
        /// whatever its age.</param>
        /// <param name="maxSets">How many sets may stay.</param>
        /// <returns>The ids of the sets to delete.</returns>
        internal static IEnumerable<string> ChooseEvictions(
            IEnumerable<(string id, long lastUsedTicks, bool isPinned)> sets,
            int maxSets
        )
        {
            var all = sets.ToList();
            var unpinnedRoom = Math.Max(0, maxSets - all.Count(set => set.isPinned));
            return all
                .Where(set => !set.isPinned)
                .OrderByDescending(set => set.lastUsedTicks)
                .Skip(unpinnedRoom)
                .Select(set => set.id)
                .ToList();
        }

        /// <summary>
        /// Gets the directory a set's inventory and summary live in.
        /// </summary>
        /// <param name="setId">The set's id.</param>
        /// <returns>The directory path.</returns>
        public static string SetDirectory(string setId) => Path.Combine(SetsDirectory, setId);

        /// <summary>
        /// Records the total patch count in the inventory for display.
        /// </summary>
        /// <param name="count">The number of patches applied this run.</param>
        public static void SetTotalPatchCount(int count)
        {
            Inventory.PatchCount = count;
        }

        /// <summary>
        /// Records the total error count in the inventory for display.
        /// </summary>
        /// <param name="count">The number of patch errors encountered this run.</param>
        public static void SetTotalErrorCount(int count)
        {
            Inventory.ErrorCount = count;
        }

        /// <summary>
        /// Records the total definition count in the inventory for display.
        /// </summary>
        /// <param name="count">The number of distinct definitions touched by patches this run.</param>
        public static void SetTotalDefinitionCount(int count)
        {
            Inventory.DefinitionCount = count;
        }

        /// <summary>
        /// Records the total new-asset count in the inventory for display.
        /// </summary>
        /// <param name="count">The number of new assets created this run.</param>
        public static void SetTotalAssetCount(int count)
        {
            Inventory.NewAssetCount = count;
        }

        /// <summary>
        /// Writes the run's summary into the patch summary log.
        /// </summary>
        /// <param name="universeSummary">The run's summary.</param>
        public static void SaveSummary(Summary universeSummary)
        {
            PatchManagerSummaryLog.UpdateCoreSummary(universeSummary.Dump());
        }

        /// <summary>
        /// Names an archive by its contents, so feature sets whose label comes out identical share one file.
        /// </summary>
        /// <param name="entries">The archive's entries as name and text pairs, in any order.</param>
        /// <returns>The archive filename.</returns>
        internal static string ContentAddressedName(IEnumerable<(string name, string text)> entries)
        {
            using var sha = SHA256.Create();
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                // BinaryWriter prefixes every string with its length, so no name and text pair can run into the next
                foreach (var (name, text) in entries.OrderBy(entry => entry.name, StringComparer.Ordinal))
                {
                    writer.Write(name);
                    writer.Write(text);
                }
            }

            stream.Position = 0;
            var digest = sha.ComputeHash(stream);
            return $"{BitConverter.ToString(digest).Replace("-", "").ToLowerInvariant()}.zip";
        }

        private static string InventoryPath(string setId) => Path.Combine(SetDirectory(setId), INVENTORY_FILENAME);

        private static string StripBackup(string name) =>
            name.EndsWith(".bak", StringComparison.OrdinalIgnoreCase) ? name.Substring(0, name.Length - 4) : name;

        private static void DeleteUnreferenced(string directory, Func<string, bool> isReferenced)
        {
            if (!Directory.Exists(directory))
                return;

            foreach (var file in Directory.GetFiles(directory))
            {
                var name = Path.GetFileName(file);
                if (isReferenced(name))
                    continue;

                if (OpenArchives.TryGetValue(name, out var archive))
                {
                    archive.Dispose();
                    OpenArchives.Remove(name);
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
