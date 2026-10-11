using KSP.Game;
using KSP.IO;
using KSP.Sim;
using Newtonsoft.Json;
using PatchManager.Core.Assets;
using PatchManager.Shared.Modules;
using PatchManager.Planets.Overrides;
using Redux.Packs;
using SpaceWarp2.API.Loading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PatchManager.Planets
{
    /// <summary>
    /// Planet patching module.
    /// </summary>
    public class PlanetsModule : BaseModule
    {
        private const string MOD_ID = "PatchManager";

        private static void RegisterAtmosphereOverride(TextAsset atmosphereOverride)
        {
            var atmosphere = JsonConvert.DeserializeObject<AtmosphereOverride>(atmosphereOverride.text);
            OverrideManager.AtmosphereOverrides[OverrideManager.GetKey(atmosphere.PlanetName, atmosphere.Layer)] =
                atmosphere;
        }

        private static void RegisterVolumeCloudOverride(TextAsset volumeCloudOverride)
        {
            var volumeCloud = IOProvider.FromJson<VolumeCloudConfigurationOverride>(volumeCloudOverride.text);
            OverrideManager.VolumeCloudOverrides[OverrideManager.GetKey(volumeCloud.bodyName, volumeCloud.Layer)] =
                volumeCloud;
        }

        /// <summary>
        /// Makes the stock galaxy, which ships under its key alone, part of the galaxy definition label's rebuild.
        /// </summary>
        /// <remarks>
        /// Every galaxy then shares one label, so a copy of the stock galaxy is patched and loaded like any other.
        /// </remarks>
        public override void Init()
        {
            PatchingManager.AddLabelMember(GalaxyDefinitionManager.GALAXY_DEFINITION_LABEL,
                SerializedSavedGame.DEFAULT_GALAXY_DEFINITION_KEY);
        }

        /// <summary>
        /// Registers the atmosphere and volume-cloud overrides as data kept per feature set, so each set's patched
        /// overrides replace the last set's whenever the set changes.
        /// </summary>
        public override void RegisterPlaySessionActions()
        {
            Loading.AddSetLoadingAction<TextAsset>(
                MOD_ID,
                "Loading atmosphere overrides",
                "atmosphere_overrides",
                RegisterAtmosphereOverride,
                OverrideManager.AtmosphereOverrides.Clear
            );
            Loading.AddSetLoadingAction<TextAsset>(
                MOD_ID,
                "Loading volume cloud overrides",
                "volume_cloud_overrides",
                RegisterVolumeCloudOverride,
                OverrideManager.VolumeCloudOverrides.Clear
            );
        }
    }
}
