using PatchManager.Shared;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace PatchManager.Core.Cache;

internal class FileResourceProvider : ResourceProviderBase
{
    /// <summary>
    /// Provides assets found by <see cref="FileResourceLocator"/>.
    /// </summary>
    /// <param name="provideHandle">Contains the data needed to provide the requested asset.</param>
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