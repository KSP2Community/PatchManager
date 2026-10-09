using KSP.Game;
using KSP.IO;
using KSP.Sim;
using Newtonsoft.Json;
using PatchManager.Core.Assets;
using PatchManager.Shared.Modules;
using PatchManager.Planets.Overrides;
using Redux.Packs;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PatchManager.Planets
{
    /// <summary>
    /// Planet patching module.
    /// </summary>
    public class PlanetsModule : BaseModule
    {
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
        /// Loads the atmosphere and volume-cloud override addressables and registers them with <c>OverrideManager</c>.
        /// </summary>
        public override void Load()
        {
            GameManager.Instance.Assets.LoadByLabel<TextAsset>(
                "atmosphere_overrides",
                RegisterAtmosphereOverride,
                Addressables.Release,
                false
            );
            GameManager.Instance.Assets.LoadByLabel<TextAsset>(
                "volume_cloud_overrides",
                RegisterVolumeCloudOverride,
                Addressables.Release,
                false
            );
        }
    }
}
