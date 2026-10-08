using System;
using KSP.Sim;
using PatchManager.CSharpPatching;
using PatchManager.CSharpPatching.Attributes;
using PatchManager.LuaPatching;
using PatchManager.Planets.UserData;
using Redux.Packs;

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

    /// <summary>Patches every galaxy definition other than the stock one. Restrict it to one galaxy with its key as the name.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PatchGalaxyAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "Galaxy";

        /// <inheritdoc />
        public override string Label => GalaxyDefinitionManager.GALAXY_DEFINITION_LABEL;
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

        /// <summary>Registers a patch against the galaxy definition with the given key.</summary>
        public static PatchBuilder<GalaxyUserData> PatchGalaxy(this PmScope scope, string galaxyDefinitionKey, string name)
            => galaxyDefinitionKey == SerializedSavedGame.DEFAULT_GALAXY_DEFINITION_KEY
                ? Patching.Build<GalaxyUserData>(scope.ModId, "Galaxy", galaxyDefinitionKey, name)
                : Patching.Build<GalaxyUserData>(scope.ModId, "Galaxy", GalaxyDefinitionManager.GALAXY_DEFINITION_LABEL, name)
                    .Named(galaxyDefinitionKey);

        /// <summary>Registers a patch against atmosphere overrides.</summary>
        public static PatchBuilder<JsonUserData> PatchAtmosphereOverride(this PmScope scope, string name)
            => Patching.Build<JsonUserData>(scope.ModId, "JSON", "atmosphere_overrides", name);

        /// <summary>Registers a patch against volume-cloud overrides.</summary>
        public static PatchBuilder<VolumeCloudUserData> PatchCloudOverride(this PmScope scope, string name)
            => Patching.Build<VolumeCloudUserData>(scope.ModId, "Cloud", "volume_cloud_overrides", name);
    }
}
