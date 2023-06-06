using JetBrains.Annotations;
using KSP.Assets;
using KSP.Game;
using PatchManager.Core.Utility;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PatchManager.Core.Preload;

public static class AssetProviderPatch
{
    public static bool OldLoadByLabel(
        string label,
        Action<object> assetLoadCallback,
        Action<IList<object>> resultCallback
    )
    {
        var locations = GameManager.Instance.Game.Assets.LocateAssetsInExternalData(label);
        if (locations.Count == 0)
        {
            Logging.LogDebug("AssetProviderPatch.LoadByLabel - No locations found.");

            // Continue with the original method
            return true;
        }

        Logging.LogDebug($"AssetProviderPatch.LoadByLabel - {locations.Count} locations found.");

        Addressables.LoadAssetsAsync(label, assetLoadCallback).Completed += results =>
        {
            if (results.Status == AsyncOperationStatus.Succeeded)
            {
                resultCallback?.Invoke(results.Result);
            }
            else
            {
                resultCallback?.Invoke(null);
                Addressables.Release(results);
            }
        };
        return false;
    }
    
    // Why is this not a static method in the original class
    [UsedImplicitly]
    public static void LoadByLabel<T>(string label, Action<T> assetLoadCallback, Action<IList<T>> resultCallback) where T : UnityEngine.Object
    {
        Logging.LogInfo($"LoadByLabel<{typeof(T).Name}>({label})");
        // At some point we should inject our code above
        
        if (AssetProvider.IsComponent(typeof(T)))
        {
            Logging.LogError("AssetProvider cannot load components/monobehaviours in batch.");
            return;
        }
        Addressables.LoadAssetsAsync(label, assetLoadCallback).Completed += delegate(AsyncOperationHandle<IList<T>> results)
        {
            if (results.Status != AsyncOperationStatus.Succeeded)
            {
                Logging.LogError("AssetProvider unable to find assets with label '" + label + "'.");
                Action<IList<T>> resultCallback2 = resultCallback;
                if (resultCallback2 != null)
                {
                    resultCallback2(null);
                }
                Addressables.Release<IList<T>>(results);
                return;
            }
            Action<IList<T>> resultCallback3 = resultCallback;
            if (resultCallback3 == null)
            {
                return;
            }
            resultCallback3(results.Result);
        };
    }
}