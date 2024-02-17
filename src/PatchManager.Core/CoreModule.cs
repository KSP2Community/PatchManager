using BepInEx;
using JetBrains.Annotations;
using KSP.Game;
using KSP.Game.Flow;
using Newtonsoft.Json;
using PatchManager.Core.Assets;
using PatchManager.Core.Cache;
using PatchManager.Core.Flow;
using PatchManager.SassyPatching.Execution;
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

    private bool _wasCacheInvalidated;

    private static bool ShouldLoad(IEnumerable<string> disabled, string modInfoLocation)
    {
        if (!File.Exists(modInfoLocation))
            return false;
        try
        {
            var metadata = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modInfoLocation));
            return metadata.ModID == null || disabled.All(x => x != metadata.ModID);
        }
        catch
        {
            return false;
        }
    }

    private static bool NoSwinfo(DirectoryInfo directory, DirectoryInfo gameRoot)
    {
        while (directory != null && directory != gameRoot)
        {
            if (directory.GetFiles().Any(x => x.Name == "swinfo.json"))
                return false;
            directory = directory.Parent;
        }

        return true;
    }

    /// <summary>
    /// Reads all patch files.
    /// </summary>
    public override void Init()
    {
        if (_shouldAlwaysInvalidate.Value || SpaceWarp.API.Mods.PluginList.ModListChangedSinceLastRun)
        {
            CacheManager.CreateCacheFolderIfNotExists();
            CacheManager.InvalidateCache();
        }

        var isValid = PatchingManager.InvalidateCacheIfNeeded();

        if (!isValid)
        {
            _wasCacheInvalidated = true;
            SpaceWarp.API.Loading.Loading.GeneralLoadingActions.Insert(0,
                () => new GenericFlowAction("Patch Manager: Creating New Assets", PatchingManager.CreateNewAssets));
            SpaceWarp.API.Loading.Loading.GeneralLoadingActions.Insert(1,
                () => new GenericFlowAction("Patch Manager: Rebuilding Cache", PatchingManager.RebuildAllCache));
            SpaceWarp.API.Loading.Loading.GeneralLoadingActions.Insert(2,
                () => new GenericFlowAction("Patch Manager: Registering Resource Locator", RegisterResourceLocator));
        }
        else
        {
            SpaceWarp.API.Loading.Loading.GeneralLoadingActions.Insert(0,
                () => new GenericFlowAction("Patch Manager: Registering Resource Locator", RegisterResourceLocator));
        }
    }

    /// <inheritdoc />
    public override void PreLoad()
    {
        var gameDataModsExists = Directory.Exists(Path.Combine(Paths.GameRootPath, "GameData/Mods"));

        // Go here instead so that the static constructor recognizes everything
        var disabledPlugins = File.ReadAllText(SpaceWarp.Preload.API.CommonPaths.DisabledPluginsFilepath)
            .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

        var modFolders = Directory.GetDirectories(Paths.PluginPath, "*", SearchOption.AllDirectories)
            .Where(dir => ShouldLoad(disabledPlugins, Path.Combine(dir, "swinfo.json")))
            .Select(x => (
                Folder: x,
                Info: JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(Path.Combine(x, "swinfo.json")))
            ))
            .ToList();

        if (gameDataModsExists)
        {
            modFolders.AddRange(
                Directory
                    .GetDirectories(Path.Combine(Paths.GameRootPath, "GameData/Mods"), "*", SearchOption.AllDirectories)
                    .Where(dir => ShouldLoad(disabledPlugins, Path.Combine(dir, "swinfo.json")))
                    .Select(x => (
                        Folder: x,
                        Info: JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(Path.Combine(x, "swinfo.json")))
                    )));
        }

        var gameRoot = new DirectoryInfo(Paths.GameRootPath);

        var standalonePatches = Directory.EnumerateFiles(
                Paths.PluginPath,
                "*.patch",
                SearchOption.AllDirectories
            )
            .Where(x => NoSwinfo(new FileInfo(x).Directory, gameRoot))
            .Select(x => new FileInfo(x))
            .ToList();

        if (gameDataModsExists)
        {
            standalonePatches.AddRange(
                Directory.EnumerateFiles(
                        Path.Combine(Paths.GameRootPath, "GameData/Mods"),
                        "*.patch",
                        SearchOption.AllDirectories
                    )
                    .Where(x => NoSwinfo(new FileInfo(x).Directory, gameRoot))
                    .Select(x => new FileInfo(x))
            );
        }

        PatchingManager.GenerateUniverse(standalonePatches.Select(x =>
            x.Directory!.FullName
                .MakeRelativePathTo(gameRoot.FullName)
                .Replace("\\", "-")
        ).ToHashSet());

        foreach (var modFolder in modFolders)
        {
            Logging.LogInfo($"Loading patchers from {modFolder.Folder}");
            // var modName = Path.GetDirectoryName(modFolder);
            PatchingManager.ImportModPatches(modFolder.Info.ModID, modFolder.Folder);
        }

        foreach (var standalonePatch in standalonePatches)
        {
            PatchingManager.ImportSinglePatch(standalonePatch);
        }

        PatchingManager.RegisterPatches();
    }

    /// <summary>
    /// Registers the provider and locator for cached assets.
    /// </summary>
    private void RegisterResourceLocator(Action resolve, Action<string> reject)
    {
        Addressables.ResourceManager.ResourceProviders.Add(new ArchiveResourceProvider());
        Locators.Register(new ArchiveResourceLocator());
        resolve();
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
        text.text += $"Total amount of errors: {PatchingManager.TotalErrorCount}\n";

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
        _shouldAlwaysInvalidate = new(modConfiguration.Bind("Core", "Always Invalidate Cache", false,
            "Should patch manager always invalidate its cache upon load"));
    }

    /// <summary>
    /// This is the current universe that patch manager is using (used for interop reasons)
    /// </summary>
    [PublicAPI]
    public static Universe CurrentUniverse => PatchingManager.Universe;
}