using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PatchManager.Core.Cache.Json;
using PatchManager.PrefabPatching;
using PatchManager.Shared;

namespace PatchManager.Core.Cache
{
    /// <summary>
    /// Cache directory and on-disk inventory of patched-asset archives. Manages archive open/create lifetimes,
    /// invalidation, and the singleton <see cref="Inventory" /> instance.
    /// </summary>
    internal static class CacheManager
    {
        /*
        private static readonly string CacheDirectory = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "cache"
        );
        */
        private const string CACHE_DIRECTORY = "./pm_cache";

        private static readonly string InventoryPath = Path.Combine(CACHE_DIRECTORY, "inventory.json");

        private static readonly Dictionary<string, Archive> OpenArchives = new();

        /// <summary>
        /// Labels whose cached archives are still valid for the current run. Consulted by the addressables resource
        /// locators to decide whether to serve patched assets from the cache.
        /// </summary>
        public static readonly List<string> CacheValidLabels = new();

        private static Inventory _inventory;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            foreach (var archive in OpenArchives.Values)
            {
                archive.Dispose();
            }

            OpenArchives.Clear();
            CacheValidLabels.Clear();
            _inventory = null;
        }

        /// <summary>
        /// Singleton <see cref="Json.Inventory" /> for the current run, lazily loaded from disk on first access.
        /// </summary>
        public static Inventory Inventory => _inventory ??= Inventory.Load(InventoryPath);

        /// <summary>
        /// Creates the cache directory on disk if it does not already exist.
        /// </summary>
        public static void CreateCacheFolderIfNotExists()
        {
            if (Directory.Exists(CACHE_DIRECTORY))
                return;

            Logging.LogDebug("Cache directory does not exist, creating a new one.");
            Directory.CreateDirectory(CACHE_DIRECTORY);
        }

        /// <summary>
        /// Creates a new cache archive at the given filename and tracks it as open.
        /// </summary>
        /// <param name="archiveFilename">The archive's filename, relative to the cache directory.</param>
        /// <returns>The newly created archive.</returns>
        /// <exception cref="ArgumentException">Thrown when an archive with that filename already exists on disk.</exception>
        public static Archive CreateArchive(string archiveFilename)
        {
            var archivePath = Path.Combine(CACHE_DIRECTORY, archiveFilename);
            if (File.Exists(archivePath))
                throw new ArgumentException($"Archive '{archivePath}' already exists!");

            var archive = new Archive(archivePath, true);
            OpenArchives.Add(archiveFilename, archive);
            return archive;
        }

        /// <summary>
        /// Loads (or returns the already-loaded copy of) the cache archive with the given filename.
        /// </summary>
        /// <param name="archiveFilename">The archive's filename, relative to the cache directory.</param>
        /// <returns>The loaded archive.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the archive does not exist on disk.</exception>
        public static Archive GetArchive(string archiveFilename)
        {
            var archivePath = Path.Combine(CACHE_DIRECTORY, archiveFilename);
            if (!File.Exists(archivePath))
                throw new FileNotFoundException($"Archive '{archivePath}' does not exist!");

            if (!OpenArchives.ContainsKey(archiveFilename))
            {
                OpenArchives.Add(archiveFilename, new Archive(archivePath));
            }

            return OpenArchives[archiveFilename];
        }

        /// <summary>
        /// Discards all cached archives, resets the inventory, and clears the cache directory.
        /// </summary>
        public static void InvalidateCache()
        {
            CacheValidLabels.Clear();

            _inventory = Inventory.Create();

            foreach (var archive in OpenArchives.Values)
            {
                archive.Dispose();
            }

            OpenArchives.Clear();

            try
            {
                DeleteDirectory(CACHE_DIRECTORY);
            }
            catch (Exception e)
            {
                Logging.LogError($"Failed to clear the patch cache: {e}");
            }

            CreateCacheFolderIfNotExists();
        }

        private static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path))
                return;

            foreach (var directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            foreach (var file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }

            Directory.Delete(path, false);
        }

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
        /// Persists the in-memory inventory to its on-disk JSON file.
        /// </summary>
        public static void SaveInventory()
        {
            Inventory.Save(InventoryPath);
        }

        public static void SaveSummary(Summary universeSummary)
        {
            PatchManagerSummaryLog.UpdateCoreSummary(universeSummary.Dump());
        }

        /// <summary>
        /// When a feature set's cache was last used, and whether it must be kept.
        /// </summary>
        internal class SetUse
        {
            /// <summary>
            /// Gets the set's directory name.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// Gets when the set was last used, in UTC ticks.
            /// </summary>
            public long LastUsedTicks { get; }

            /// <summary>
            /// Gets whether the set is kept whatever its age, as a pack's or a save's set is.
            /// </summary>
            public bool IsPinned { get; }

            /// <summary>
            /// Records one set's use.
            /// </summary>
            /// <param name="id">The set's directory name.</param>
            /// <param name="lastUsedTicks">When the set was last used, in UTC ticks.</param>
            /// <param name="isPinned">Whether the set is kept whatever its age.</param>
            public SetUse(string id, long lastUsedTicks, bool isPinned)
            {
                Id = id;
                LastUsedTicks = lastUsedTicks;
                IsPinned = isPinned;
            }
        }

        /// <summary>
        /// Picks the feature set caches to delete so that at most <paramref name="maxSets" /> unpinned ones remain,
        /// least recently used first.
        /// </summary>
        /// <param name="sets">Every set on disk.</param>
        /// <param name="maxSets">How many sets may stay.</param>
        /// <returns>The ids of the sets to delete.</returns>
        internal static IEnumerable<string> ChooseEvictions(IEnumerable<SetUse> sets, int maxSets) =>
            Array.Empty<string>();

        /// <summary>
        /// Names an archive by its contents, so feature sets whose label comes out identical share one file.
        /// </summary>
        /// <param name="entries">The archive's entries as name and text pairs, in any order.</param>
        /// <returns>The archive filename.</returns>
        internal static string ContentAddressedName(IEnumerable<(string name, string text)> entries) => "";
    }
}
