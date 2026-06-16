using System;
using PatchManager.CSharpPatching;
using PatchManager.CSharpPatching.Attributes;
using PatchManager.Parts.UserData;

namespace PatchManager.Parts
{
    /// <summary>Patches part definitions (Part converter, parts_data label), mirroring PM.Parts.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchPartAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "Part";

        /// <inheritdoc />
        public override string Label => "parts_data";
    }

    /// <summary>Fluent PM extensions for part patches.</summary>
    public static class PartsPatchExtensions
    {
        /// <summary>Registers a part patch.</summary>
        public static PatchBuilder<PartUserData> PatchPart(this PmScope scope, string name)
            => Patching.Build<PartUserData>(scope.ModId, "Part", "parts_data", name);
    }
}
