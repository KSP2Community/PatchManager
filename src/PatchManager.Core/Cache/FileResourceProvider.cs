using PatchManager.Core.Utility;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace PatchManager.Core.Cache;

internal class FileResourceProvider : ResourceProviderBase
{
    public override void Provide(ProvideHandle provideHandle)
    {
        try
        {
            Logging.LogDebug($"Loading file {provideHandle.Location.InternalId}");
            var asset = new TextAsset(File.ReadAllText(provideHandle.Location.InternalId));
            provideHandle.Complete(asset, true, null);
        }
        catch (Exception e)
        {
            Logging.LogError(e);
            provideHandle.Complete<AssetBundle>(null, false, e);
        }
    }
}