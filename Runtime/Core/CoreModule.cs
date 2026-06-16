using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using KSP.Game;
using PatchManager.Core.Assets;
using PatchManager.Core.Cache;
using PatchManager.LuaPatching;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using ReduxLib.Configuration;
using ReduxLib.Configuration.Attributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        /// <summary>
        /// Decides whether to invalidate the cache, then schedules the patch-loading flow actions for the SpaceWarp loader.
        /// </summary>
        public override void Init()
        {
            ConfigReplay.ReplayAll();

            if (Application.isEditor || _shouldAlwaysInvalidate ||
                SpaceWarp2.API.Mods.PluginList.ModListChangedSinceLastRun ||
                ConfigReplay.HasStaleConfigs)
            {
                CacheManager.CreateCacheFolderIfNotExists();
                CacheManager.InvalidateCache();
            }

            PatchingManager.HashScriptFiles();
            var isValid = PatchingManager.InvalidateCacheIfNeeded();

            // Every mod body has already run in SpaceWarp's per-plugin script phase, which precedes
            // GeneralLoadingActions on both warm and cold launches, so close the Lua patch-definition window here.
            SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(0,
                () => new FlowAction("Patch Manager: Closing Registration", CloseRegistration));

            if (!isValid)
            {
                _wasCacheInvalidated = true;
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(1, () =>
                    new FlowAction("Patch Manager: Collecting script results", CollectScriptResults));
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(2,
                    () => new FlowAction("Patch Manager: Registering all patches", RegisterAllPatches));
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(3,
                    () => new FlowAction("Patch Manager: Creating New Assets", PatchingManager.CreateNewAssets));
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(4,
                    () => new FlowAction("Patch Manager: Rebuilding Cache", PatchingManager.RebuildAllCache));
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(5,
                    () => new FlowAction("Patch Manager: Saving Patch Summary", SavePatchSummary));
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(6,
                    () => new FlowAction("Patch Manager: Registering Resource Locator", RegisterResourceLocator));
            }
            else
            {
                SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(1,
                    () => new FlowAction("Patch Manager: Registering Resource Locator", RegisterResourceLocator));
            }
        }

        private static void CloseRegistration(Action resolve, Action<string> reject)
        {
            PatchingManager.Universe.RegistrationOpen = false;
            resolve();
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

        private static void CollectScriptResults(Action resolve, Action<string> reject)
        {
            PatchingManager.CollectScriptResults();
            resolve();
        }

        /// <inheritdoc />
        public override void PreLoad()
        {
            // Discovery and body-running now belong to SpaceWarp's mod runtime. PatchManager only needs the
            // universe to exist before those bodies run, so their PM:Patch calls have somewhere to register.
            PatchingManager.GenerateUniverse(new HashSet<string>());
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