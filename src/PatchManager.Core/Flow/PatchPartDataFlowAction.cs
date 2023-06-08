using PatchManager.Core.Assets;
using PatchManager.Shared;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PatchManager.Core.Flow;

/// <summary>
/// Loads all part data from the game and replaces it with the data from the patch folder.
/// </summary>
public class PatchPartDataFlowAction : IAction
{
    private const string Label = "parts_data";

    private readonly string _cachePath;
    private readonly string _patchesPath;

    /// <inheritdoc/>
    public string Name => "JSON Manager: patching part data";

    /// <summary>
    /// Creates an instance of the action.
    /// </summary>
    /// <param name="cachePath">Path to the folder where the patched files will be cached.</param>
    /// <param name="patchesPath">Path to the folder from where patches will be loaded.</param>
    public PatchPartDataFlowAction(string cachePath, string patchesPath)
    {
        _cachePath = cachePath;
        _patchesPath = patchesPath;
    }

    /// <inheritdoc/>
    public void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            Logging.LogDebug("JSON Manager patching started");

            Addressables.LoadAssetsAsync(
                Label,
                new Action<TextAsset>(OnJsonLoaded)
            ).Completed += OnCompleted(resolve, reject);
        }
        catch (Exception e)
        {
            Logging.LogError(e);
            reject($"JSON Manager part patching failed: {e.Message}");
        }
    }

    private Action<AsyncOperationHandle<IList<TextAsset>>> OnCompleted(Action resolve, Action<string> reject) =>
        operation =>
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                var files = Directory.EnumerateFiles(_cachePath);
                Addresses.Register(Label, files);
                Logging.LogDebug("JSON Manager part patching completed");
                resolve();
            }
            else
            {
                Logging.LogError(operation.OperationException);
                reject($"JSON Manager part patching failed: {operation.OperationException.Message}");
            }
        };

    private void OnJsonLoaded(TextAsset asset)
    {
        if (asset == null)
        {
            return;
        }

        var cacheJson = asset.text;

        var patchJsonPath = Path.Combine(_patchesPath, $"{asset.name}.json");
        if (File.Exists(patchJsonPath))
        {
            cacheJson = File.ReadAllText(patchJsonPath);
            Logging.LogDebug($"Patching {asset.name}");
        }

        var cacheJsonPath = Path.Combine(_cachePath, $"{asset.name}.json");
        File.WriteAllText(cacheJsonPath, cacheJson);
        Addresses.Register(asset.name, cacheJsonPath);
    }
}