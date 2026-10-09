using System.Collections.Generic;
using JetBrains.Annotations;
using PatchManager.Planets.Overrides;
using Redux.Packs;

namespace PatchManager.Planets
{
    [PublicAPI]
    public static class OverrideManager
    {
        public static Dictionary<(string body, string layer), AtmosphereOverride> AtmosphereOverrides = new();
        public static Dictionary<string, double> Scales = new();
        public static Dictionary<(string body, string layer), VolumeCloudConfigurationOverride> VolumeCloudOverrides = new();

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            AtmosphereOverrides = new();
            Scales = new();
            VolumeCloudOverrides = new();
        }

        /// <summary>
        /// Gets the key an override for a body in a layer is stored under.
        /// </summary>
        /// <param name="bodyName">The body's name, in any case.</param>
        /// <param name="layer">The override's layer, or null for the default layer.</param>
        /// <returns>The key.</returns>
        public static (string body, string layer) GetKey(string bodyName, string layer) =>
            (bodyName.ToLowerInvariant(), CampaignPack.IsDefaultLayer(layer) ? CampaignPack.DEFAULT_LAYER : layer);

        /// <summary>
        /// Gets the atmosphere override for a body taken from a layer.
        /// </summary>
        /// <param name="bodyName">The body's name, in any case.</param>
        /// <param name="layer">The layer the galaxy takes the body from.</param>
        /// <param name="atmosphereOverride">The layer's override, or else the default layer's.</param>
        /// <returns>True if either layer has an override for the body, false otherwise.</returns>
        public static bool TryGetAtmosphereOverride(string bodyName, string layer, out AtmosphereOverride atmosphereOverride) =>
            TryGetLayered(AtmosphereOverrides, bodyName, layer, out atmosphereOverride);

        /// <summary>
        /// Gets the volume cloud override for a body taken from a layer.
        /// </summary>
        /// <param name="bodyName">The body's name, in any case.</param>
        /// <param name="layer">The layer the galaxy takes the body from.</param>
        /// <param name="volumeCloudOverride">The layer's override, or else the default layer's.</param>
        /// <returns>True if either layer has an override for the body, false otherwise.</returns>
        public static bool TryGetVolumeCloudOverride(string bodyName, string layer,
            out VolumeCloudConfigurationOverride volumeCloudOverride) =>
            TryGetLayered(VolumeCloudOverrides, bodyName, layer, out volumeCloudOverride);

        // A layer's override wins, as a layer's copy of body data does, and the default layer's stands in otherwise
        private static bool TryGetLayered<T>(Dictionary<(string body, string layer), T> overrides, string bodyName,
            string layer, out T value) =>
            overrides.TryGetValue(GetKey(bodyName, layer), out value) ||
            overrides.TryGetValue(GetKey(bodyName, null), out value);
    }
}
