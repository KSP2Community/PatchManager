using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Canonical JSON and SHA-256 helpers shared by the editor compiler, resolver,
/// cache, and fluent frontend.
/// </summary>
public static class PrefabPatchJson
{
    /// <summary>
    /// Canonical serializer settings used for manifests, plans, and hashes.
    /// </summary>
    public static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Include,
        Converters = { new StringEnumConverter() }
    };

    /// <summary>
    /// Serializes a value using the canonical prefab-patch JSON format.
    /// </summary>
    /// <param name="value">Value to serialize.</param>
    /// <returns>Formatted canonical JSON.</returns>
    public static string Serialize(object value) =>
        JsonConvert.SerializeObject(value, Settings);

    /// <summary>
    /// Deserializes canonical prefab-patch JSON.
    /// </summary>
    /// <param name="json">JSON to deserialize.</param>
    /// <typeparam name="T">Expected result type.</typeparam>
    /// <returns>The deserialized value.</returns>
    public static T Deserialize<T>(string json) =>
        JsonConvert.DeserializeObject<T>(json, Settings);

    /// <summary>
    /// Calculates the lowercase SHA-256 digest of a UTF-8 string.
    /// </summary>
    /// <param name="value">Value to hash; null is treated as an empty string.</param>
    /// <returns>A lowercase hexadecimal digest.</returns>
    public static string Sha256(string value)
    {
        using var algorithm = SHA256.Create();
        var bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value ?? ""));
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Calculates a manifest content hash without including its current hash field.
    /// </summary>
    /// <param name="manifest">Manifest to hash.</param>
    /// <returns>The canonical manifest hash.</returns>
    public static string CalculateManifestHash(PrefabPatchManifest manifest)
    {
        var previous = manifest.ManifestHash;
        manifest.ManifestHash = null;
        try
        {
            return Sha256(Serialize(manifest));
        }
        finally
        {
            manifest.ManifestHash = previous;
        }
    }
}
