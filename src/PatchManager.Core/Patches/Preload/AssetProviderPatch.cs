using JetBrains.Annotations;
using KSP.Assets;
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
        Logging.LogInfo($"LoadByLabel<{typeof(T).Name}>({label})");
        // At some point we should inject our code above

        if (AssetProvider.IsComponent(typeof(T)))
        {
            Logging.LogError("AssetProvider cannot load components/MonoBehaviours in batch.");
            return;
        }

        Addressables.LoadAssetsAsync(label, assetLoadCallback).Completed += results =>
        {
            if (results.Status != AsyncOperationStatus.Succeeded)
            {
                Logging.LogError($"AssetProvider unable to find assets with label '{label}'.");
                resultCallback?.Invoke(null);
                Addressables.Release(results);
                return;
            }

            resultCallback?.Invoke(results.Result);
        };
    }
}