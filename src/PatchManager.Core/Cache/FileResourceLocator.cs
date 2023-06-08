using PatchManager.Core.Assets;
using PatchManager.Shared;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PatchManager.Core.Cache;

internal class FileResourceLocator : IResourceLocator
{
    /// <summary>
    /// The ID of the locator.
    /// </summary>
    public string LocatorId => GetType().FullName;
    /// <summary>
    /// The labels registered in the locator.
    /// </summary>
    public IEnumerable<object> Keys => Addresses.RegisteredAddresses.Keys;

    /// <summary>
    /// Locates an asset file by its label and type.
    /// </summary>
    /// <param name="key">Label of the asset to be located.</param>
    /// <param name="type">Type of the asset to be located.</param>
    /// <param name="locations">Locations of found assets.</param>
    /// <returns>True if any assets were found, false otherwise.</returns>
    public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
    {
        if (type != typeof(TextAsset))
        {
            locations = new List<IResourceLocation>();
            return false;
        }

        locations = (
            from assetList in Addresses.RegisteredAddresses
            let assetKey = assetList.Key
            where assetKey == key
            from asset in assetList.Value
            select new ResourceLocationBase(
                (string)assetKey,
                asset,
                typeof(FileResourceProvider).FullName,
                typeof(TextAsset)
            ) as IResourceLocation
        ).ToList();

        if (locations.Count == 0)
        {
            return false;
        }

        Logging.LogDebug($"Located key '{key}' with {locations.Count} locations.");
        return true;
    }
}