using BepInEx;
using JetBrains.Annotations;
using PatchManager.Core.Assets;
using PatchManager.Core.Cache;
using PatchManager.Core.Cache.Json;
using PatchManager.Core.Utility;
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
    /// Reads all patch files.
    /// </summary>
    public override void Preload()
    {
        // TODO: Move this whole process into a SpaceWarp 1.3 per-mod flow action

        var modFolders = Directory.GetDirectories(Paths.PluginPath, "*", SearchOption.TopDirectoryOnly);

        foreach (var modFolder in modFolders)
        {
            var modName = Path.GetDirectoryName(modFolder);
            PatchingManager.RunModPatches(modName, modFolder);
        }

        PatchingManager.InvalidateCacheIfNeeded();
    }

    /// <summary>
    /// Registers the provider and locator for cached assets.
    /// </summary>
    public override void Load()
    {
        Logging.LogInfo("Registering resource locator");
        Addressables.ResourceManager.ResourceProviders.Add(new ArchiveResourceProvider());
        Locators.Register(new ArchiveResourceLocator());
    }
}