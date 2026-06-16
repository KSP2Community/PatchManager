using System;
using PatchManager.CSharpPatching;
using PatchManager.CSharpPatching.Attributes;
using PatchManager.LuaPatching;

namespace PatchManager.Generic
{
    /// <summary>
    /// Patches an arbitrary addressables label through the generic JSON converter, mirroring PM.Json. Use this for
    /// asset kinds that have no dedicated domain attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PatchJsonAttribute : PatchAttribute
    {
        private readonly string _label;

        /// <param name="label">The addressables label whose assets this patch targets.</param>
        public PatchJsonAttribute(string label) => _label = label;

        /// <inheritdoc />
        public override string Converter => "JSON";

        /// <inheritdoc />
        public override string Label => _label;
    }

    /// <summary>Fluent PM extensions for generic JSON patches.</summary>
    public static class GenericPatchExtensions
    {
        /// <summary>Registers a JSON patch against the given addressables label.</summary>
        public static PatchBuilder<JsonUserData> PatchJson(this PmScope scope, string label, string name)
            => Patching.Build<JsonUserData>(scope.ModId, "JSON", label, name);
    }
}
