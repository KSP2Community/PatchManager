using System.Reflection;
using System.Runtime.CompilerServices;

namespace PatchManager.CSharpPatching
{
    /// <summary>
    /// Static entrypoint for fluent C# patches written outside a mod instance.
    /// </summary>
    /// <remarks>
    /// Inside a mod that derives from KerbalMod, use the instance <c>PM</c> property instead. Domain-specific patch
    /// methods (PatchPart, PatchPlanet, ...) are extension methods on <see cref="PmScope" />, shipped with each domain.
    /// </remarks>
    public static class Patching
    {
        /// <summary>A patch scope bound to the calling assembly's mod ID.</summary>
        public static PmScope Mod
        {
            // NoInlining keeps GetCallingAssembly resolving to the caller's assembly, not an inlined-away frame.
            [MethodImpl(MethodImplOptions.NoInlining)]
            get => new(PatchModId.Resolve(Assembly.GetCallingAssembly()));
        }

        /// <summary>A patch scope bound to an explicit mod ID.</summary>
        /// <param name="modId">The mod ID patches built from the scope are namespaced under.</param>
        /// <returns>A patch scope bound to <paramref name="modId" />.</returns>
        public static PmScope ForMod(string modId) => new(modId);

        /// <summary>
        /// Builds a typed fluent patch under the given mod ID, converter, and label.
        /// </summary>
        /// <remarks>
        /// The primitive the domain and mod extension methods call into.
        /// </remarks>
        /// <param name="modId">The mod ID the patch is namespaced under.</param>
        /// <param name="converter">The name of the converter that produces the asset wrapper.</param>
        /// <param name="label">The label selecting which assets the patch targets.</param>
        /// <param name="name">The patch name, unique within the mod ID.</param>
        /// <typeparam name="T">The asset wrapper type the patch's converter produces.</typeparam>
        /// <returns>A typed fluent builder for the queued patch.</returns>
        public static PatchBuilder<T> Build<T>(string modId, string converter, string label, string name)
            => new(FluentPatchRegistry.Build(modId, converter, label, name));
    }

    /// <summary>
    /// A mod-scoped fluent patch context carrying the mod ID.
    /// </summary>
    /// <remarks>
    /// Domain-specific patch methods (PatchPart, PatchPlanet, ...) are extension methods on this type, so any domain
    /// or mod can add its own.
    /// </remarks>
    public readonly struct PmScope
    {
        /// <summary>The mod ID patches built from this scope are namespaced under.</summary>
        public readonly string ModId;

        internal PmScope(string modId) => ModId = modId;
    }
}
