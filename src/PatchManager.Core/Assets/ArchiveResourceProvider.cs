using PatchManager.Core.Cache;
using PatchManager.Shared;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace PatchManager.Core.Assets;

/// <summary>
/// Provides assets found by <see cref="ArchiveResourceLocator"/>.
/// </summary>
internal class ArchiveResourceProvider : ResourceProviderBase
{
    /// <summary>
    /// Provides assets found by <see cref="ArchiveResourceLocator"/>.
    /// </summary>
    /// <param name="provideHandle"></param>
    public override void Provide(ProvideHandle provideHandle)
    {
        try
        {
            var archiveName = provideHandle.Location.PrimaryKey;
            var file = provideHandle.Location.InternalId;
            Logging.LogDebug($"Loading {archiveName}/{file}");

            var archive = CacheManager.GetArchive(archiveName);
            var asset = new TextAsset(archive.ReadFile(file));
            provideHandle.Complete(asset, true, null);
        }
        catch (Exception e)
        {
            Logging.LogError(e);
            provideHandle.Complete<AssetBundle>(null, false, e);
        }
    }
}