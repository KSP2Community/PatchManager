using PatchManager.Core.Cache;
using PatchManager.Shared;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PatchManager.Core.Assets;

/// <summary>
/// Locates assets in archives of cached assets.
/// </summary>
internal class ArchiveResourceLocator : IResourceLocator
{
    /// <summary>
    /// The ID of the locator.
    /// </summary>
    public string LocatorId => GetType().FullName;

    /// <summary>
    /// The labels registered in the locator.
    /// </summary>
    public IEnumerable<object> Keys => CacheManager.Inventory.CacheEntries.Keys;

    /// <summary>
    /// Locates an asset file in a cache archive by its label and type.
    /// </summary>
    /// <param name="key">Label to find.</param>
    /// <param name="type">Type of assets to find.</param>
    /// <param name="locations">List of found locations.</param>
    /// <returns></returns>
    public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
    {
        var label = key.ToString();
        if (!CacheManager.Inventory.CacheEntries.TryGetValue(label, out var cacheEntry))
        {
            locations = new List<IResourceLocation>();
            return false;
        }

        locations = cacheEntry.Assets
            .Select(asset => new ResourceLocationBase(
                cacheEntry.ArchiveFilename,
                asset,
                typeof(ArchiveResourceProvider).FullName,
                typeof(TextAsset)
            ))
            .Cast<IResourceLocation>()
            .ToList();

        if (locations.Count == 0)
        {
            return false;
        }

        Logging.LogDebug($"Located key '{key}' with {locations.Count} locations.");
        return true;
    }
}