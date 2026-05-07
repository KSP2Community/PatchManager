using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.Core.Assets;
using PatchManager.Shared;
using UniLinq;

namespace PatchManager.Core.Cache.Json
{
    /// <summary>
    /// Catalog of all patched labels and their respective archives, persisted to disk between runs.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Inventory
    {
        /// <summary>
        /// Dictionary of all patched labels and their respective archives and assets.
        /// </summary>
        [JsonProperty("cache", Required = Required.Always)]
        public Dictionary<string, CacheEntry> CacheEntries { get; internal set; }

        /// <summary>
        /// Checksum hash of the patch_hashes field.
        /// </summary>
        [JsonProperty("checksum", Required = Required.Always)]
        public string Checksum { get; internal set; }

        /// <summary>
        /// Dictionary of all patches and their hashes.
        /// </summary>
        [JsonProperty("patch_hashes", Required = Required.Always)]
        public PatchHashes Patches { get; internal set; }

        /// <summary>
        /// Total number of patches applied during the most recent rebuild.
        /// </summary>
        [JsonProperty("patch_count")] public int PatchCount { get; internal set; } = 0;

        /// <summary>
        /// Total number of patch errors during the most recent rebuild.
        /// </summary>
        [JsonProperty("error_count")] public int ErrorCount { get; internal set; } = 0;

        /// <summary>
        /// Total number of distinct asset definitions touched by patches during the most recent rebuild.
        /// </summary>
        [JsonProperty("definition_count")] public int DefinitionCount { get; internal set; } = 0;

        /// <summary>
        /// Total number of new assets created during the most recent rebuild.
        /// </summary>
        [JsonProperty("new_asset_count")] public int NewAssetCount { get; internal set; } = 0;

        /// <summary>
        /// Per-mod replay slices for <c>Config:</c> bindings, keyed by mod ID. Each slice carries identity
        /// (descriptor vs standalone + .lua path) and the bindings the mod's patches declared. Lets the settings
        /// UI surface bindings on hot-cache launches when the patch scripts don't run, and lets
        /// <see cref="ConfigReplay" /> compare current values against the value the patch ran with to decide
        /// whether to invalidate.
        /// </summary>
        [JsonProperty("serialized_configs")]
        public Dictionary<string, ConfigReplaySlice> SerializedConfigs { get; internal set; } = new();

        /// <summary>
        /// Get a <see cref="CacheEntry" /> by its label.
        /// </summary>
        /// <param name="label">Asset label to get the entry for.</param>
        /// <returns>A pair of asset label and instance of <see cref="CacheEntry" /> if found, otherwise the default pair.</returns>
        public KeyValuePair<string, CacheEntry> GetByLabel(string label)
        {
            return CacheEntries.FirstOrDefault(
                entry => entry.Key == label
            );
        }

        /// <summary>
        /// Get a <see cref="CacheEntry" /> by its archive's name.
        /// </summary>
        /// <param name="archiveFilename">Archive filename to get the entry for.</param>
        /// <returns>A pair of asset label and instance of <see cref="CacheEntry" /> if found, otherwise the default pair.</returns>
        public KeyValuePair<string, CacheEntry> GetByArchive(string archiveFilename)
        {
            return CacheEntries.FirstOrDefault(
                entry => entry.Value.ArchiveFilename == archiveFilename
            );
        }

        /// <summary>
        /// Creates a fresh empty inventory.
        /// </summary>
        /// <returns>A new inventory with no cache entries.</returns>
        internal static Inventory Create()
        {
            return new Inventory
            {
                CacheEntries = new Dictionary<string, CacheEntry>()
            };
        }

        /// <summary>
        /// Loads an inventory from the given path; creates a fresh one if the file is missing or corrupt.
        /// </summary>
        /// <param name="path">Path to the inventory JSON file.</param>
        /// <returns>The loaded inventory, or a fresh one on error.</returns>
        internal static Inventory Load(string path)
        {
            CacheManager.CreateCacheFolderIfNotExists();
            if (!File.Exists(path))
            {
                Logging.LogDebug($"Inventory file does not exist, creating new inventory.");
                return Create();
            }

            try
            {
                var inventoryText = File.ReadAllText(path);
                var inventory = JsonConvert.DeserializeObject<Inventory>(inventoryText);
                Logging.LogDebug("Inventory file loaded successfully.");
                return inventory;
            }
            catch (Exception e)
            {
                Logging.LogError($"Inventory file was corrupted: {e.Message}");
                return Create();
            }
        }

        /// <summary>
        /// Persists this inventory to the given path as JSON.
        /// </summary>
        /// <param name="path">Path to write the inventory JSON file to.</param>
        internal void Save(string path)
        {
            CacheManager.CreateCacheFolderIfNotExists();
            var formatting = PatchingManager.UseIndentedOutput ? Formatting.Indented : Formatting.None;
            var inventoryText = JsonConvert.SerializeObject(this, formatting);
            File.WriteAllText(path, inventoryText);
        }
    }
}
