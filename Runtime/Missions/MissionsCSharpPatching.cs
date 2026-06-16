using System;
using PatchManager.CSharpPatching;
using PatchManager.CSharpPatching.Attributes;
using PatchManager.Missions.UserData;

namespace PatchManager.Missions
{
    /// <summary>Patches mission definitions, mirroring PM.Missions.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchMissionAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "Mission";

        /// <inheritdoc />
        public override string Label => "missions";
    }

    /// <summary>Fluent PM extensions for mission patches.</summary>
    public static class MissionsPatchExtensions
    {
        /// <summary>Registers a mission patch.</summary>
        public static PatchBuilder<MissionUserData> PatchMission(this PmScope scope, string name)
            => Patching.Build<MissionUserData>(scope.ModId, "Mission", "missions", name);
    }
}
