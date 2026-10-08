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

        /// <summary>Registers a patch that copies each matching asset under a new name in the same label. Do runs on the copy.</summary>
        public static PatchBuilder<PartUserData> DuplicatePart(this PmScope scope, string source, string newName)
            => Patching.Duplicate<PartUserData>(scope.ModId, "Part", "parts_data", source, newName);
    }
}
