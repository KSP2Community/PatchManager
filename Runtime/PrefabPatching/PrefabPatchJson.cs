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

    public static string Serialize(object value) =>
        JsonConvert.SerializeObject(value, Settings);

    public static T Deserialize<T>(string json) =>
        JsonConvert.DeserializeObject<T>(json, Settings);

    public static string Sha256(string value)
    {
        using var algorithm = SHA256.Create();
        var bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value ?? ""));
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }

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
