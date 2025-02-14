using System.Collections.Generic;
using JetBrains.Annotations;
using KSP.Game;
using Newtonsoft.Json.Linq;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using Unity.VisualScripting;
using UnityEngine.AddressableAssets;
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
        /// Implement the resource units from https://github.com/KSP2Community/CommunityResources
        /// </summary>
        internal static readonly Dictionary<string, string> ResourceUnits = new();


        public override void Load()
        {
            GameManager.Instance.Assets.LoadByLabel("resource_units", RegisterUnits,
                delegate(IList<TextAsset> assetLocations)
                {
                    if (assetLocations != null)
                    {
                        Addressables.Release(assetLocations);
                    }
                });
            
            PatchManager.Instance.AddComponent<NonStageableResourcesUIController>();
        }


        private static void RegisterUnits(TextAsset textAsset)
        {
            var dict = JObject.Parse(textAsset.text);
            foreach (var value in dict)
            {
                if (value.Value is not { Type: JTokenType.String }) continue;
                ResourceUnits[value.Key] = value.Value.Value<string>()!;
            }
        }
    }
}