using PatchManager.Core.Utility;
using KSP.Game.Flow;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PatchManager.Core.Flow;

public class PatchPartDataFlowAction : FlowAction
{
    private const string Label = "parts_data";

    private readonly string _cachePath;
    private readonly string _patchesPath;

    public PatchPartDataFlowAction(string cachePath, string patchesPath)
        : base("JSON Manager: patching part data")
    {
        _cachePath = cachePath;
        _patchesPath = patchesPath;
    }

    public override void DoAction(Action resolve, Action<string> reject)
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