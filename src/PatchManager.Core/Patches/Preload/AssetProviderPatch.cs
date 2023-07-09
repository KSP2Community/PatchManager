using JetBrains.Annotations;
using KSP.Game;
using PatchManager.Core.Assets;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityObject = UnityEngine.Object;

namespace PatchManager.Core.Patches.Preload;

internal static class AssetProviderPatch
{
    [UsedImplicitly]
    public static AsyncOperationHandle<IList<T>> LoadAssetsAsync<T>(
        string key,
        Action<T> assetLoadCallback
    )
    {
        if (Locators.LocateAll(key, out var patchedLocations))
        {
            return Addressables.LoadAssetsAsync(patchedLocations, assetLoadCallback);
        }

        var locations = GameManager.Instance.Game.Assets.LocateAssetsInExternalData(key);
        if (locations.Count <= 0)
        {
            return Addressables.LoadAssetsAsync(key, assetLoadCallback);
        }

        foreach (var resourceLocator in Addressables.ResourceLocators)
        {
            if (resourceLocator.Locate(key, typeof(T), out var internalLocations))
            {
                locations.AddRange(internalLocations);
            }
        }

        return Addressables.LoadAssetsAsync(locations, assetLoadCallback);
    }
}