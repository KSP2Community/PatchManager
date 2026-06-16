using System;
using PatchManager.CSharpPatching;
using PatchManager.CSharpPatching.Attributes;
using PatchManager.LuaPatching;

namespace PatchManager.Resources
{
    /// <summary>
    /// Patches resource and recipe definitions, mirroring PM.Resources. The Resource converter yields either a
    /// resource or a recipe wrapper depending on the asset, so the typed surface uses the shared JsonUserData base.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchResourceAttribute : PatchAttribute
    {
        /// <inheritdoc />
        public override string Converter => "Resource";

        /// <inheritdoc />
        public override string Label => "resources";
    }

    /// <summary>Fluent PM extensions for resource patches.</summary>
    public static class ResourcesPatchExtensions
    {
        /// <summary>Registers a resource or recipe patch.</summary>
        public static PatchBuilder<JsonUserData> PatchResource(this PmScope scope, string name)
            => Patching.Build<JsonUserData>(scope.ModId, "Resource", "resources", name);
    }
}
