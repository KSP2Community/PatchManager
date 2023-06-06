using PatchManager.Core.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PatchManager.Core.Cache;

internal class FileResourceLocator : IResourceLocator
{
    public string LocatorId => GetType().FullName;
    public IEnumerable<object> Keys => Addresses.RegisteredAddresses.Keys;

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