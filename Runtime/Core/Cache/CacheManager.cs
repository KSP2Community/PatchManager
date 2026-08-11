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
        /// Labels whose cached archives are still valid for the current run; consulted by the addressables resource
        /// locators to decide whether to serve patched assets from the cache.
        /// </summary>
        public static readonly List<string> CacheValidLabels = new();

        private static Inventory _inventory;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
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
            {
                return;
            }

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
            {
                throw new ArgumentException($"Archive '{archivePath}' already exists!");
            }

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
            {
                throw new FileNotFoundException($"Archive '{archivePath}' does not exist!");
            }

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
            {
                return;
            }

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
    }
}
