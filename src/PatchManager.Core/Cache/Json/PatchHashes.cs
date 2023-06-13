using Newtonsoft.Json;
using PatchManager.Core.Utility;

namespace PatchManager.Core.Cache.Json;

/// <summary>
/// Contains hashes of the cache.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class PatchHashes
{
    /// <summary>
    /// KSP 2 version for which the cache was generated.
    /// </summary>
    [JsonProperty("ksp2_version", Required = Required.Always)]
    public string Ksp2Version { get; internal set; }

    /// <summary>
    /// PatchManager version for which the cache was generated.
    /// </summary>
    [JsonProperty("patch_manager_version", Required = Required.Always)]
    public string PatchManagerVersion { get; internal set; }

    /// <summary>
    /// Dictionary of all patches and their hashes.
    /// </summary>
    [JsonProperty("patches", Required = Required.Always)]
    public Dictionary<string, string> Patches { get; internal set; }

    /// <summary>
    /// Create a default instance of <see cref="PatchHashes"/> with current KSP 2 and Patch Manager versions.
    /// </summary>
    /// <returns>Default instance of <see cref="PatchHashes"/></returns>
    public static PatchHashes CreateDefault()
    {
        return new PatchHashes
        {
            Ksp2Version = Versions.Ksp2Version,
            PatchManagerVersion = Versions.PatchManagerVersion,
            Patches = new Dictionary<string, string>()
        };
    }
}