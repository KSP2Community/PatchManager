using BepInEx;
using JetBrains.Annotations;
using KSP.Game;
using KSP.Game.Flow;
using Newtonsoft.Json;
using PatchManager.Core.Assets;
using PatchManager.Core.Cache;
using PatchManager.Core.Flow;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Mods.JSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace PatchManager.Core;

/// <summary>
/// Core module for PatchManager.
/// </summary>
[UsedImplicitly]
public class CoreModule : BaseModule
{
    private ConfigValue<bool> _shouldAlwaysInvalidate;

    private bool _wasCacheInvalidated = false;
    private static bool ShouldLoad(string[] disabled, string modInfoLocation)
    {
        if (!File.Exists(modInfoLocation))
            return false;
        try
        {
            var metadata = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modInfoLocation));
            return metadata.ModID == null || !disabled.Contains(metadata.ModID);
        } catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Reads all patch files.
    /// </summary>
    public override void Preload()
    {
        // Go here instead so that the static constructor recognizes everything
        PatchingManager.GenerateUniverse();
        var disabledPlugins = File.ReadAllText(Path.Combine(Paths.BepInExRootPath, "disabled_plugins.cfg"))
            .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        var modFolders = Directory.GetDirectories(Paths.PluginPath, "*",SearchOption.AllDirectories)
            .Where(dir => ShouldLoad(disabledPlugins, Path.Combine(dir, "swinfo.json")));

        foreach (var modFolder in modFolders)
        {
            Logging.LogInfo($"Loading patchers from {modFolder}");
            var modName = Path.GetDirectoryName(modFolder);
            PatchingManager.ImportModPatches(modName, modFolder);
        }

        PatchingManager.RegisterPatches();

        if (_shouldAlwaysInvalidate.Value)
        {
            CacheManager.CreateCacheFolderIfNotExists();
            CacheManager.InvalidateCache();
        }
        
        var isValid = PatchingManager.InvalidateCacheIfNeeded();

        if (!isValid)
        {
            _wasCacheInvalidated = true;
            SpaceWarp.API.Loading.Loading.AddGeneralLoadingAction(
                () => new GenericFlowAction("Patch Manager: Creating New Assets", PatchingManager.CreateNewAssets));
            SpaceWarp.API.Loading.Loading.AddGeneralLoadingAction(
                () => new GenericFlowAction("Patch Manager: Rebuilding Cache", PatchingManager.RebuildAllCache));
        }
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

    /// <inheritdoc />
    public override VisualElement GetDetails()
    {
        var foldout = new Foldout
        {
            text = "PatchManager.Core",
            style =
            {
                display = DisplayStyle.Flex
            },
            visible = true
        };
        var text = new TextElement();
        text.text += $"Amount of loaded patchers: {PatchingManager.Patchers.Count}\n";
        text.text += $"Amount of loaded generators: {PatchingManager.Generators.Count}\n";
        text.text += $"Amount of loaded libraries: {PatchingManager.Universe.AllLibraries.Count}\n";
        if (_wasCacheInvalidated)
        {
            text.text += $"Total amount of patches: {PatchingManager.TotalPatchCount}\n";
        }
        else
        {
            text.text += "Total amount of patches: Unknown (loaded from cache)\n";
        }

        text.text += "Patched labels:";
        foreach (var label in PatchingManager.Universe.LoadedLabels)
        {
            text.text += $"\n- {label}";
        }

        text.visible = true;
        text.style.display = DisplayStyle.Flex;
        foldout.Add(text);
        
        return foldout;
    }

    /// <inheritdoc />
    public override void BindConfiguration(IConfigFile modConfiguration)
    {
        _shouldAlwaysInvalidate = new (modConfiguration.Bind("Core", "Always Invalidate Cache", false,
            "Should patch manager always invalidate its cache upon load"));
    }
}