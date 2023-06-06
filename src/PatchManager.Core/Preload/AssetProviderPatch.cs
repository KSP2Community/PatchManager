using KSP.Game;
using PatchManager.Core.Utility;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PatchManager.Core.Preload;

internal static class AssetProviderPatch
{
    public static bool LoadByLabel(
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
}