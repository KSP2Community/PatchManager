using JetBrains.Annotations;
using KSP.Assets;
using PatchManager.Core.Assets;
using PatchManager.Shared;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PatchManager.Core.Patches.Preload;

internal static class AssetProviderPatch
{
    /// <summary>
    /// Used by the preloader patcher to replace the body of <see cref="AssetProvider.LoadByLabel{T}"/>
    /// </summary>
    /// <param name="label">Label of the assets to be loaded.</param>
    /// <param name="assetLoadCallback">Action to execute for each loaded asset.</param>
    /// <param name="resultCallback">Action to execute after loading is done.</param>
    /// <typeparam name="T">Type of assets to load.</typeparam>
    [UsedImplicitly]
    public static void LoadByLabel<T>(string label, Action<T> assetLoadCallback, Action<IList<T>> resultCallback)
        where T : UnityEngine.Object
    {
        if (AssetProvider.IsComponent(typeof(T)))
        {
            Logging.LogError("AssetProvider cannot load components/MonoBehaviours in batch.");
            return;
        }

        var onCompletedCallback = new Action<AsyncOperationHandle<IList<T>>>(results =>
        {
            if (results.Status != AsyncOperationStatus.Succeeded)
            {
                Logging.LogError($"AssetProvider unable to find assets with label '{label}'.");
                resultCallback?.Invoke(null);
                Addressables.Release(results);
                return;
            }

            resultCallback?.Invoke(results.Result);
        });

        var found = Locators.LocateAll(label, typeof(T), out var locations);

        if (found)
        {
            Logging.LogDebug($"Found {locations.Count} custom locations with label '{label}'.");
            Addressables.LoadAssetsAsync(locations, assetLoadCallback).Completed += onCompletedCallback;
            return;
        }

        Addressables.LoadAssetsAsync(label, assetLoadCallback).Completed += onCompletedCallback;
    }
}