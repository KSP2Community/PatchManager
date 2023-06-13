using Newtonsoft.Json;

namespace PatchManager.Core.Cache.Json;

/// <summary>
/// Represents an entry in the cache for a specific asset label.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class CacheEntry
{
    /// <summary>
    /// Label of the asset(s).
    /// </summary>
    [JsonProperty("label", Required = Required.Always)]
    public string Label { get; internal set; }

    /// <summary>
    /// Filename of the archive containing the asset(s).
    /// </summary>
    [JsonProperty("archive", Required = Required.Always)]
    public string ArchiveFilename { get; internal set; }

    /// <summary>
    /// List of all assets in the archive.
    /// </summary>
    [JsonProperty("assets", Required = Required.Always)]
    public List<string> Assets { get; internal set; }
}