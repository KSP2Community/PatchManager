using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using KSP.Game;
using Newtonsoft.Json;
using PatchManager.Core.Assets;
using PatchManager.Core.Cache;
using PatchManager.LuaPatching;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using ReduxLib.Configuration;
using ReduxLib.Configuration.Attributes;
using SpaceWarp2.API.Mods.JSON;
using UniLinq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using FlowAction = PatchManager.Core.Flow.FlowAction;

namespace PatchManager.Core
{
    /// <summary>
    /// Core module for PatchManager.
    /// </summary>
    [UsedImplicitly]
    public class CoreModule : BaseModule
    {
        private const string PATCH_LABEL = "redux_patches";
        private const string REDUX_MOD_ID = "Redux";

        [ConfigSection("Advanced", loc: "Menu/Settings/Sections/Advanced")]
        [ConfigValue("Always Invalidate Patch Manager Cache",
            "Should patch manager always invalidate its cache upon load",
            nameLoc: "Menu/Settings/AlwaysInvalidatePmCache",
            descLoc: "Menu/Settings/Description/AlwaysInvalidatePmCache")]
        private bool _shouldAlwaysInvalidate;

        [ConfigValue("Indent Patched JSON",
            "Format patched JSON output with indentation in the cache. Useful for inspection but is slightly slower. Always enabled in the unity editor",
            nameLoc: "Menu/Settings/IndentPatchedJson",
            descLoc: "Menu/Settings/Description/IndentPatchedJson")]
        private bool _indentedPatchOutput;

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
        /// Decides whether to invalidate the cache, then schedules the patch-loading flow actions for the SpaceWarp loader.
        /// </summary>
        public override void Init()
        {
            if (Application.isEditor || _shouldAlwaysInvalidate ||
                SpaceWarp2.API.Mods.PluginList.ModListChangedSinceLastRun)
            {
                CacheManager.CreateCacheFolderIfNotExists();
                CacheManager.InvalidateCache();
            }

            var isValid = PatchingManager.InvalidateCacheIfNeeded();

            if (!isValid)
            {
                _wasCacheInvalidated = true;
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(0, () =>
                    new FlowAction("Patch Manager: loading Patches from Addressables",
                        LoadPatchesFromAddressables));
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(1,
                    () => new FlowAction("Patch Manager: Registering all patches", RegisterAllPatches));
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(2,
                    () => new FlowAction("Patch Manager: Creating New Assets", PatchingManager.CreateNewAssets));
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(3,
                    () => new FlowAction("Patch Manager: Rebuilding Cache", PatchingManager.RebuildAllCache));
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(4,
                    () => new FlowAction("Patch Manager: Saving Patch Summary", SavePatchSummary));
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(5,
                    () => new FlowAction("Patch Manager: Registering Resource Locator", RegisterResourceLocator));
            }
            else
            {
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(0,
                    () => new FlowAction("Patch Manager: Registering Resource Locator", RegisterResourceLocator));
            }
        }

        private void SavePatchSummary(Action resolve, Action<string> reject)
        {
            PatchingManager.Universe.Summary.RecognizedModIds = PatchingManager.Universe.AllMods;
            CacheManager.SaveSummary(PatchingManager.Universe.Summary);
            resolve();
        }

        private static void RegisterAllPatches(Action resolve, Action<string> reject)
        {
            PatchingManager.RegisterPatches();
            resolve();
        }

        private static void LoadPatchesFromAddressables(Action resolve, Action<string> reject)
        {
            var handle = GameManager.Instance.Assets.LoadAssetsAsync<TextAsset>(PATCH_LABEL,
                asset => { PatchingManager.ImportAssetPatch(asset, REDUX_MOD_ID); });
            handle.Completed += result =>
            {
                if (result.Status == AsyncOperationStatus.Succeeded)
                    resolve();
                else
                    reject("Failed to load patch assets!");
            };
        }

        /// <inheritdoc />
        public override void PreLoad()
        {
            // Go here instead so that the static constructor recognizes everything
            var disabledPlugins = File.ReadAllText(SpaceWarp2.API.CommonPaths.DisabledPlugins)
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

            var modFolders = Directory
                .GetDirectories(SpaceWarp2.API.CommonPaths.ModsFolder, "*", SearchOption.AllDirectories)
                .Where(dir => ShouldLoad(disabledPlugins, Path.Combine(dir, "swinfo.json")))
                .Select(x => (
                    Folder: x,
                    Info: JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(Path.Combine(x, "swinfo.json")))
                ))
                .ToList();

            var gameRoot = new DirectoryInfo(".");

            var standalonePatches = Directory.EnumerateFiles(
                    SpaceWarp2.API.CommonPaths.ModsFolder,
                    "*.patch",
                    SearchOption.AllDirectories
                )
                .Where(x => NoSwinfo(new FileInfo(x).Directory, gameRoot))
                .Select(x => new FileInfo(x))
                .ToList();


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
        }

        /// <summary>
        /// Registers the provider and locator for cached assets.
        /// </summary>
        private void RegisterResourceLocator(Action resolve, Action<string> reject)
        {
            Addressables.ResourceManager.ResourceProviders.Add(new ArchiveResourceProvider());
            Locators.Register(new ArchiveResourceLocator());
            // Perfect place to load the patch manager information from the old inventory as well
            GameManager.Instance.Game.UI.UitkLoadingCurtain.Data.PatchManagerDefinitionsModifiedCount =
                CacheManager.Inventory.DefinitionCount;
            GameManager.Instance.Game.UI.UitkLoadingCurtain.Data.PatchManagerNewAssetCount =
                CacheManager.Inventory.NewAssetCount;
            GameManager.Instance.Game.UI.UitkLoadingCurtain.Data.PatchManagerPatchCount =
                CacheManager.Inventory.PatchCount;
            GameManager.Instance.Game.UI.UitkLoadingCurtain.Data.PatchManagerErrorCount =
                CacheManager.Inventory.ErrorCount;
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
            text.text += $"Amount of loaded patchers: {PatchingManager.Universe.TotalPatchCount}\n";
            text.text += $"Amount of loaded generators: {PatchingManager.Universe.AllNewAssets.Count}\n";
            text.text += $"Amount of loaded libraries: {PatchingManager.Universe.LibraryCount}\n";
            if (_wasCacheInvalidated)
            {
                text.text += $"Total amount of patches: {PatchingManager.TotalPatchCount}\n";
                text.text += $"Total amount of errors: {PatchingManager.TotalErrorCount}\n";
            }
            else
            {
                text.text += $"Total amount of patches: {CacheManager.Inventory.PatchCount}\n";
                text.text += $"Total amount of errors: {CacheManager.Inventory.ErrorCount}\n";
            }

            text.text += "Patched labels:";
            foreach (var label in PatchingManager.Universe.PatchedLabels)
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
            modConfiguration.Bind(this);
            PatchingManager.UseIndentedOutput = Application.isEditor || _indentedPatchOutput;
        }

        /// <summary>
        /// The current universe that patch manager is using (exposed for interop).
        /// </summary>
        [PublicAPI]
        public static Universe CurrentUniverse => PatchingManager.Universe;
    }
}