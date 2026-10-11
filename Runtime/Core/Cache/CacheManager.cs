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
    /// The patch cache of the feature set being patched for: its inventory, the archives it reads, and the labels
    /// still valid for this run.
    /// </summary>
    internal static class CacheManager
    {
        private const string CACHE_DIRECTORY = "./pm_cache";

        // Set caches kept beyond the pinned ones, least recently used evicted first
        private const int MAX_SETS = 10;

        /// <summary>
        /// Labels whose cached archives are still valid for the current run. Consulted by the addressables resource
        /// locators to decide whether to serve patched assets from the cache.
        /// </summary>
        public static readonly List<string> CacheValidLabels = new();

        private static CacheStore _store;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _store?.Dispose();
            _store = null;
            CacheValidLabels.Clear();
        }

        /// <summary>
        /// Gets the cache on disk.
        /// </summary>
        public static CacheStore Store => _store ??= new CacheStore(CACHE_DIRECTORY);

        /// <summary>
        /// Gets the inventory of the set being patched for.
        /// </summary>
        public static Inventory Inventory => Store.Inventory;

        /// <summary>
        /// Opens the cache of the set being patched for.
        /// </summary>
        /// <param name="setId">The set's id.</param>
        public static void UseSet(string setId)
        {
            CacheValidLabels.Clear();
            Store.UseSet(setId);
        }

        /// <summary>
        /// Opens a stored archive, or returns the copy already open.
        /// </summary>
        /// <param name="archiveFilename">The archive's filename.</param>
        /// <returns>The loaded archive.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the archive does not exist on disk.</exception>
        public static Archive GetArchive(string archiveFilename) => Store.GetArchive(archiveFilename);

        /// <summary>
        /// Discards the cache of the set being patched for, leaving every other set's alone.
        /// </summary>
        public static void InvalidateCache()
        {
            CacheValidLabels.Clear();
            Store.InvalidateSet();
        }

        /// <summary>
        /// Deletes the least recently used set caches above the limit, and whatever only they used.
        /// </summary>
        public static void EvictStaleSets()
        {
            Store.Evict(MAX_SETS, new HashSet<string>());
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
            Store.SaveInventory();
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
        internal static IEnumerable<string> ChooseEvictions(IEnumerable<SetUse> sets, int maxSets)
        {
            var all = sets.ToList();
            var unpinnedRoom = Math.Max(0, maxSets - all.Count(set => set.IsPinned));
            return all
                .Where(set => !set.IsPinned)
                .OrderByDescending(set => set.LastUsedTicks)
                .Skip(unpinnedRoom)
                .Select(set => set.Id)
                .ToList();
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
    }
}
