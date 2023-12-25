using JetBrains.Annotations;
using KSP.Assets;
using KSP.Game;
using PatchManager.Core.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityObject = UnityEngine.Object;

namespace PatchManager.Core.Patches.Preload;

internal static class AssetProviderPatch
{
    [UsedImplicitly]
    public static void LoadByLabel<T>(
        string label,
        Action<T> assetLoadCallback,
        Action<IList<T>> resultCallback = null
    ) where T : UnityObject
    {
        if (AssetProvider.IsComponent(typeof(T)))
        {
            Debug.LogError("AssetProvider cannot load components/monobehaviours in batch.");
            return;
        }

        LoadPatchedAssetsAsync(label, assetLoadCallback).Completed += results =>
        {
            if (results.Status == AsyncOperationStatus.Succeeded)
            {
                resultCallback?.Invoke(results.Result);
                return;
            }

            Debug.LogError("AssetProvider unable to find assets with label '" + label + "'.");
            resultCallback?.Invoke(null);
            Addressables.Release((AsyncOperationHandle)results);
        };
    }

    private static AsyncOperationHandle<IList<T>> LoadPatchedAssetsAsync<T>(
        string key,
        Action<T> assetLoadCallback
    ) => Locators.LocateAll(key, out var patchedLocations)
        ? Addressables.LoadAssetsAsync(patchedLocations, assetLoadCallback)
        : GameManager.Instance.Assets.LoadAssetsAsync(key, assetLoadCallback);
}