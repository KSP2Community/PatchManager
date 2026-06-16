using System.Reflection;
using System.Runtime.CompilerServices;

namespace PatchManager.CSharpPatching
{
    /// <summary>
    /// Static entrypoint for fluent C# patches written outside a mod instance. Inside a mod that derives from
    /// KerbalMod, use the instance <c>PM</c> property instead. Domain-specific patch methods
    /// (PatchPart, PatchPlanet, ...) are extension methods on <see cref="PmScope" />, shipped with each domain.
    /// </summary>
    public static class Patching
    {
        /// <summary>A patch scope bound to the calling assembly's mod ID.</summary>
        public static PmScope Mod
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get => new(PatchModId.Resolve(Assembly.GetCallingAssembly()));
        }

        /// <summary>A patch scope bound to an explicit mod ID.</summary>
        public static PmScope ForMod(string modId) => new(modId);

        /// <summary>
        /// Builds a typed fluent patch under the given mod ID, converter, and label. The primitive the domain and
        /// mod extension methods call into.
        /// </summary>
        public static PatchBuilder<T> Build<T>(string modId, string converter, string label, string name)
            => new(FluentPatchRegistry.Build(modId, converter, label, name));
    }

    /// <summary>
    /// A mod-scoped fluent patch context carrying the mod ID. Domain-specific patch methods (PatchPart, PatchPlanet,
    /// ...) are extension methods on this type, so each domain - and any mod - can add its own.
    /// </summary>
    public readonly struct PmScope
    {
        /// <summary>The mod ID patches built from this scope are namespaced under.</summary>
        public readonly string ModId;

        internal PmScope(string modId) => ModId = modId;
    }
}
