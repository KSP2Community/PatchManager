using Newtonsoft.Json;
using PatchManager.Shared;

namespace PatchManager.Core.Cache.Json;

/// <summary>
/// Serves as a catalog of all patched labels and their respective archives.
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
    /// Get a <see cref="CacheEntry"/> by its label.
    /// </summary>
    /// <param name="label">Asset label to get the entry for.</param>
    /// <returns>A pair of asset label and instance of <see cref="CacheEntry"/> if found, otherwise null.</returns>
    public KeyValuePair<string, CacheEntry> GetByLabel(string label)
    {
        return CacheEntries.FirstOrDefault(
            entry => entry.Key == label
        );
    }

    /// <summary>
    /// Get a <see cref="CacheEntry"/> by its archive's name.
    /// </summary>
    /// <param name="archiveFilename">Archive filename to get the entry for.</param>
    /// <returns>A pair of asset label and instance of <see cref="CacheEntry"/> if found, otherwise null.</returns>
    public KeyValuePair<string, CacheEntry> GetByArchive(string archiveFilename)
    {
        return CacheEntries.FirstOrDefault(
            entry => entry.Value.ArchiveFilename == archiveFilename
        );
    }

    internal static Inventory Create()
    {
        return new Inventory
        {
            CacheEntries = new Dictionary<string, CacheEntry>()
        };
    }

    internal static Inventory Load(string path)
    {
        CacheManager.CreateCacheFolderIfNotExists();
        if (!File.Exists(path))
        {
            return Create();
        }

        try
        {
            var inventoryText = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Inventory>(inventoryText);
        }
        catch (Exception e)
        {
            Logging.LogError($"Inventory file was corrupted: {e.Message}");
            return Create();
        }
    }

    internal void Save(string path)
    {
        CacheManager.CreateCacheFolderIfNotExists();
        var inventoryText = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(path, inventoryText);
    }
}