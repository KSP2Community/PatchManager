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
        /// Schedules the post-body cache-validity decision.
        /// </summary>
        /// <remarks>
        /// The decision needs the config values mod bodies bind, so it runs in <see cref="DecideCacheValidity" />
        /// (after the per-plugin body phase) rather than here in Init, which runs in PM's Awake, before any body.
        /// </remarks>
        public override void Init()
        {
            RegisterLoadingActions();
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterAssembliesLoaded
        )]
        private static void RestoreLoadingActionsWithoutDomainReload()
        {
            // SpaceWarp resets GeneralLoadingActions at SubsystemRegistration.
            // With domain reload disabled PatchManager's MonoBehaviour and module
            // instances survive, so Awake/Init do not run again to repopulate it.
            // AfterAssembliesLoaded runs after that reset and before SpaceWarp
            // builds the new play session's loading flow.
            foreach (var module in ModuleManager.Modules)
            {
                if (module is CoreModule core)
                {
                    core.RegisterLoadingActions();
                    return;
                }
            }
        }

        private void RegisterLoadingActions()
        {
            SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(0,
                () => new FlowAction("Patch Manager: Closing Registration", CloseRegistration));
            SpaceWarp2.API.Loading.Loading.GeneralLoadingActions.Insert(1,
                () => new FlowAction("Patch Manager: Deciding Cache Validity", DecideCacheValidity));
        }

        private void DecideCacheValidity(Action resolve, Action<string> reject)
        {
            if (Application.isEditor || _shouldAlwaysInvalidate ||
                SpaceWarp2.API.Mods.PluginList.ModListChangedSinceLastRun ||
                PatchingManager.TaggedConfigChanged())
            {
                CacheManager.CreateCacheFolderIfNotExists();
                CacheManager.InvalidateCache();
            }

            PatchingManager.HashScriptFiles();
            var isValid = PatchingManager.InvalidateCacheIfNeeded();

            var tail = new List<GenericFlowAction>();
            if (!isValid)
            {
                _wasCacheInvalidated = true;
                tail.Add(new GenericFlowAction("Patch Manager: Collecting script results", CollectScriptResults));
                tail.Add(new GenericFlowAction("Patch Manager: Registering all patches", RegisterAllPatches));
                tail.Add(new GenericFlowAction("Patch Manager: Creating New Assets", PatchingManager.CreateNewAssets));
                tail.Add(new GenericFlowAction("Patch Manager: Rebuilding Cache", PatchingManager.RebuildAllCache));
                tail.Add(new GenericFlowAction("Patch Manager: Saving Invalidation Snapshot", PatchingManager.SaveInvalidationSnapshot));
                tail.Add(new GenericFlowAction("Patch Manager: Saving Patch Summary", SavePatchSummary));
            }

            tail.Add(
                new GenericFlowAction(
                    "Patch Manager: Resolving Prefab Patch Plans",
                    ResolvePrefabPatchPlans
                )
            );
            tail.Add(new GenericFlowAction("Patch Manager: Registering Resource Locator", RegisterResourceLocator));

            // Splice the tail in right after this step. Insert back-to-front so each Insert at the same index
            // pushes the previous one down, leaving the tail in its original order.
            var insertIndex = GameManager.Instance.LoadingFlow.flowIndex + 1;
            for (var i = tail.Count - 1; i >= 0; i--)
            {
                GameManager.Instance.LoadingFlow.FlowActions.Insert(insertIndex, tail[i]);
            }

            resolve();
        }

        private static void CloseRegistration(Action resolve, Action<string> reject)
        {
            PatchingManager.Universe.RegistrationOpen = false;
            PrefabPatchRuntime.CloseRegistration();
            resolve();
        }

        private static void ResolvePrefabPatchPlans(
            Action resolve,
            Action<string> reject
        )
        {
            var catalogOwners = PluginList.AllEnabledAndActivePlugins
                .SelectMany(descriptor =>
                    descriptor.AddressableResourceLocators.Select(locator =>
                        new
                        {
                            locator.LocatorId,
                            descriptor.Guid
                        }
                    )
                )
                .GroupBy(value => value.LocatorId, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group =>
                        group.Select(value => value.Guid)
                            .Distinct(StringComparer.Ordinal)
                            .Single(),
                    StringComparer.Ordinal
                );
            PrefabPatchRuntime.DiscoverAndResolve(
                PatchingManager.Universe.AllMods,
                catalogOwners,
                resolve,
                reject
            );
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
