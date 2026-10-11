using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using PatchManager.Shared.Modules;
using SpaceWarp2.API.Loading;
using UnityEngine;

namespace PatchManager.Resources
{
    /// <summary>
    /// Resource patching module.
    /// </summary>
    [UsedImplicitly]
    public class ResourcesModule : BaseModule
    {
        /// <summary>
        /// Implements the resource units from https://github.com/KSP2Community/CommunityResources.
        /// </summary>
        internal static readonly Dictionary<string, string> ResourceUnits = new();


        /// <summary>
        /// Registers the <c>resource_units</c> addressables as data kept per feature set, so each set's patched
        /// units replace the last set's.
        /// </summary>
        public override void RegisterPlaySessionActions()
        {
            Loading.AddSetLoadingAction<TextAsset>(
                "PatchManager",
                "resource units",
                "resource_units",
                RegisterUnits,
                ResourceUnits.Clear
            );
        }

        /// <summary>
        /// Adds the non-stageable resources UI controller.
        /// </summary>
        public override void Load()
        {
            // Host on a dedicated persistent GameObject rather than PatchManager.Instance: PatchManager is
            // a KerbalMod whose GameObject is destroyed between Play Mode sessions when Domain Reload is
            // disabled, so AddComponent on it throws MissingReferenceException on a later play. The
            // controller is standalone. Guard against adding a duplicate within a session.
            if (Object.FindAnyObjectByType<NonStageableResourcesUIController>() == null)
            {
                var host = new GameObject(nameof(NonStageableResourcesUIController));
                Object.DontDestroyOnLoad(host);
                host.AddComponent<NonStageableResourcesUIController>();
            }
        }


        private static void RegisterUnits(TextAsset textAsset)
        {
            var dict = JObject.Parse(textAsset.text);
            foreach (var value in dict)
            {
                if (value.Value is not { Type: JTokenType.String })
                    continue;

                ResourceUnits[value.Key] = value.Value.Value<string>()!;
            }
        }
    }
}
