using System;
using PatchManager.CSharpPatching;
using PatchManager.CSharpPatching.Attributes;
using PatchManager.LuaPatching;
using PatchManager.Planets.UserData;

namespace PatchManager.Planets
{
    /// <summary>Patches celestial bodies, mirroring PM.Planets.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchPlanetAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "Planet";

        /// <inheritdoc />
        public override string Label => "celestial_bodies";
    }

    /// <summary>Patches the default galaxy definition.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchDefaultGalaxyAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "Galaxy";

        /// <inheritdoc />
        public override string Label => "GalaxyDefinition_Default";
    }

    /// <summary>Patches atmosphere overrides.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchAtmosphereOverrideAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "JSON";

        /// <inheritdoc />
        public override string Label => "atmosphere_overrides";
    }

    /// <summary>Patches volume-cloud overrides.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchCloudOverrideAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "Cloud";

        /// <inheritdoc />
        public override string Label => "volume_cloud_overrides";
    }

    /// <summary>Fluent PM extensions for planet patches.</summary>
    public static class PlanetsPatchExtensions
    {
        /// <summary>Registers a celestial-body patch.</summary>
        public static PatchBuilder<CelestialBodyUserData> PatchPlanet(this PmScope scope, string name)
            => Patching.Build<CelestialBodyUserData>(scope.ModId, "Planet", "celestial_bodies", name);

        /// <summary>Registers a patch against the default galaxy definition.</summary>
        public static PatchBuilder<GalaxyUserData> PatchDefaultGalaxy(this PmScope scope, string name)
            => Patching.Build<GalaxyUserData>(scope.ModId, "Galaxy", "GalaxyDefinition_Default", name);

        /// <summary>Registers a patch against atmosphere overrides.</summary>
        public static PatchBuilder<JsonUserData> PatchAtmosphereOverride(this PmScope scope, string name)
            => Patching.Build<JsonUserData>(scope.ModId, "JSON", "atmosphere_overrides", name);

        /// <summary>Registers a patch against volume-cloud overrides.</summary>
        public static PatchBuilder<VolumeCloudUserData> PatchCloudOverride(this PmScope scope, string name)
            => Patching.Build<VolumeCloudUserData>(scope.ModId, "Cloud", "volume_cloud_overrides", name);
    }
}
