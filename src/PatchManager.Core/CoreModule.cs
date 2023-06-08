using JetBrains.Annotations;
using KSP.Game;
using PatchManager.Core.Cache;
using PatchManager.Core.Flow;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using UnityEngine.AddressableAssets;

namespace PatchManager.Core;

/// <summary>
/// Core module for PatchManager.
/// </summary>
[UsedImplicitly]
public class CoreModule : BaseModule
{
    /// <summary>
    /// Registers the PatchPartDataFlowAction.
    /// </summary>
    public override void Preload()
    {
        FlowManager.RegisterAction(new PatchPartDataFlowAction(
            PatchManagerPlugin.CachePath,
            PatchManagerPlugin.PatchesPath
        ));
    }

    /// <summary>
    /// Registers the FileResourceProvider and FileResourceLocator.
    /// </summary>
    public override void Load()
    {
        Logging.LogInfo("Registering resource locator");
        Addressables.ResourceManager.ResourceProviders.Add(new FileResourceProvider());
        GameManager.Instance.Game.Assets.RegisterResourceLocator(new FileResourceLocator());
    }
}