using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using KSP.Game;
using KSP.Game.Flow;
using PatchManager.Core.Assets;
using PatchManager.Core.Cache;
using PatchManager.LuaPatching;
using PatchManager.PrefabPatching;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using ReduxLib.Configuration;
using ReduxLib.Configuration.Attributes;
using SpaceWarp2.API.Mods;
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
        /// Registers the post-body cache-validity decision for the current play session.
        /// </summary>
        /// <remarks>
        /// The decision needs the scripts mod bodies ran and the features they declared, so it runs in
        /// <see cref="DecideCacheValidity" /> (after the per-plugin body phase) rather than during one-time module
        /// initialization, which runs before any body.
        /// </remarks>
        public override void RegisterPlaySessionActions()
        {
            _wasCacheInvalidated = false;
            SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(0,
                () => new FlowAction("Patch Manager: Closing Registration", CloseRegistration));
            SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(1,
                () => new FlowAction("Patch Manager: Deciding Cache Validity", DecideCacheValidity));
        }

        private void DecideCacheValidity(Action resolve, Action<string> reject)
        {
            // Both passes feed the checksum, so both run before the decision reads it. Mod bodies have already
            // run at this point, so CollectScriptResults finds the addressable-sourced scripts in memory.
            PatchingManager.HashScriptFiles();
            PatchingManager.CollectScriptResults();
            PatchingManager.AlwaysInvalidate = Application.isEditor || _shouldAlwaysInvalidate;
            var flow = GameManager.Instance.LoadingFlow;
            var tail = PatchingManager.SetRunSteps(
                next => PatchingManager.InsertNext(flow, next),
                out _wasCacheInvalidated
            );
            tail.Add(new GenericFlowAction("Patch Manager: Registering Resource Locator", RegisterResourceLocator));
            PatchingManager.InsertNext(flow, tail);
            resolve();
        }

        private static void CloseRegistration(Action resolve, Action<string> reject)
        {
            PatchingManager.Universe.RegistrationOpen = false;
            PrefabPatchRuntime.CloseRegistration();
            PatchingManager.SelectBootFeatures();
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
            // Only add the provider if absent: RegisterResourceLocator is a loading flow action that runs
            // each Play Mode enter, so a plain Add would accumulate duplicate providers if the list
            // persists across sessions (Domain Reload disabled). The Locators registry is cleared per play.
            var providers = Addressables.ResourceManager.ResourceProviders;
            bool hasArchiveProvider = false;
            for (int i = 0; i < providers.Count; i++)
            {
                if (providers[i].GetType() == typeof(ArchiveResourceProvider))
                {
                    hasArchiveProvider = true;
                    break;
                }
            }

            if (!hasArchiveProvider)
            {
                providers.Add(new ArchiveResourceProvider());
            }

            Locators.Register(new ArchiveResourceLocator());
            Locators.Register(PrefabPatchRuntime.RegisterResourceProvider());
            PatchingManager.ShowInventoryOnLoadingScreen();
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

            var prefabMetrics = PrefabPatchRuntime.CurrentMetrics;
            text.text +=
                $"\nPrefab plans: {prefabMetrics.ResolvedPlanCount}"
                + $" ({prefabMetrics.CacheHitCount} cache hit(s), "
                + $"{prefabMetrics.CacheMissCount} miss(es))";
            text.text +=
                $"\nRetained prefab handles: "
                + $"{prefabMetrics.RetainedAddressablesHandles}";

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
