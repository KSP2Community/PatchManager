using System.Collections.Generic;
using JetBrains.Annotations;
using PatchManager.Planets.Overrides;

namespace PatchManager.Planets
{
    [PublicAPI]
    public static class OverrideManager
    {
        // Keyed by body name in lowercase, since renderers name bodies in lowercase
        public static Dictionary<string, AtmosphereOverride> AtmosphereOverrides = new();
        public static Dictionary<string, double> Scales = new();
        public static Dictionary<string, VolumeCloudConfigurationOverride> VolumeCloudOverrides = new();

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            AtmosphereOverrides.Clear();
            Scales = new();
            VolumeCloudOverrides.Clear();
        }

        /// <summary>
        /// Gets the atmosphere override for a body.
        /// </summary>
        /// <param name="bodyName">The body's name, in any case.</param>
        /// <param name="atmosphereOverride">The override.</param>
        /// <returns>True if the body has an override, false otherwise.</returns>
        public static bool TryGetAtmosphereOverride(string bodyName, out AtmosphereOverride atmosphereOverride) =>
            AtmosphereOverrides.TryGetValue(bodyName.ToLowerInvariant(), out atmosphereOverride);

        /// <summary>
        /// Gets the volume cloud override for a body.
        /// </summary>
        /// <param name="bodyName">The body's name, in any case.</param>
        /// <param name="volumeCloudOverride">The override.</param>
        /// <returns>True if the body has an override, false otherwise.</returns>
        public static bool TryGetVolumeCloudOverride(string bodyName,
            out VolumeCloudConfigurationOverride volumeCloudOverride) =>
            VolumeCloudOverrides.TryGetValue(bodyName.ToLowerInvariant(), out volumeCloudOverride);
    }
}
