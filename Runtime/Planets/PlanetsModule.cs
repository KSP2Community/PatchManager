using System.Collections.Generic;
using JetBrains.Annotations;
using KSP.Game;
using KSP.IO;
using Newtonsoft.Json;
using PatchManager.Shared.Modules;
using PatchManager.Planets.Overrides;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PatchManager.Planets
{
    public class PlanetsModule : BaseModule
    {
        private static void RegisterAtmosphereOverride(TextAsset atmosphereOverride)
        {
            var atmosphere = JsonConvert.DeserializeObject<AtmosphereOverride>(atmosphereOverride.text);
            OverrideManager.AtmosphereOverrides[atmosphere.PlanetName] = atmosphere;
        }

        private static void RegisterVolumeCloudOverride(TextAsset volumeCloudOverride)
        {
            var volumeCloud = IOProvider.FromJson<VolumeCloudConfigurationOverride>(volumeCloudOverride.text);
            OverrideManager.VolumeCloudOverrides[volumeCloud.bodyName.ToLowerInvariant()] = volumeCloud;
        }
    
        /// <summary>
        /// Runs when the mod is first initialized.
        /// </summary>
        public override void Load()
        {
            GameManager.Instance.Assets.LoadByLabel("atmosphere_overrides", RegisterAtmosphereOverride,
                delegate(IList<TextAsset> assetLocations) { Addressables.Release(assetLocations); });
            GameManager.Instance.Assets.LoadByLabel("volume_cloud_overrides", RegisterVolumeCloudOverride, 
                delegate(IList<TextAsset> assetLocations) { Addressables.Release(assetLocations); });
        }
    }
}