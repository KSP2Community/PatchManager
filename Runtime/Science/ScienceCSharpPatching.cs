using System;
using PatchManager.CSharpPatching;
using PatchManager.CSharpPatching.Attributes;
using PatchManager.LuaPatching;
using PatchManager.Science.UserData;

namespace PatchManager.Science
{
    /// <summary>Patches science-region discoverables, mirroring PM.Science.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchDiscoverablesAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "Discoverables";

        /// <inheritdoc />
        public override string Label => "science_region_discoverables";
    }

    /// <summary>Patches science experiments.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchExperimentAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "Experiment";

        /// <inheritdoc />
        public override string Label => "scienceExperiment";
    }

    /// <summary>Patches science regions.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchScienceRegionAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "ScienceRegions";

        /// <inheritdoc />
        public override string Label => "science_region";
    }

    /// <summary>Patches tech-tree nodes through the generic JSON converter.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchTechNodeAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "JSON";

        /// <inheritdoc />
        public override string Label => "techNodeData";
    }

    /// <summary>Fluent PM extensions for science patches.</summary>
    public static class SciencePatchExtensions
    {
        /// <summary>Registers a science-region discoverables patch.</summary>
        public static PatchBuilder<DiscoverablesUserData> PatchDiscoverables(this PmScope scope, string name)
            => Patching.Build<DiscoverablesUserData>(scope.ModId, "Discoverables", "science_region_discoverables", name);

        /// <summary>Registers a science-experiment patch.</summary>
        public static PatchBuilder<ExperimentUserData> PatchExperiment(this PmScope scope, string name)
            => Patching.Build<ExperimentUserData>(scope.ModId, "Experiment", "scienceExperiment", name);

        /// <summary>Registers a science-region patch.</summary>
        public static PatchBuilder<ScienceRegionsUserData> PatchScienceRegion(this PmScope scope, string name)
            => Patching.Build<ScienceRegionsUserData>(scope.ModId, "ScienceRegions", "science_region", name);

        /// <summary>Registers a tech-tree node patch.</summary>
        public static PatchBuilder<JsonUserData> PatchTechNode(this PmScope scope, string name)
            => Patching.Build<JsonUserData>(scope.ModId, "JSON", "techNodeData", name);
    }
}
