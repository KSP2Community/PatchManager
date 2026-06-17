using System;

namespace PatchManager.CSharpPatching.Attributes
{
    /// <summary>
    /// Marks a class as a C# patch group. Its methods carrying a per-domain patch attribute are discovered and
    /// registered with the universe at load time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PMPatchAttribute : Attribute
    {
        /// <summary>
        /// The explicit mod ID patches in this class are namespaced under, or null to derive it from the
        /// declaring assembly (its SpaceWarp plugin, or Redux for game code).
        /// </summary>
        public string ModId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PMPatchAttribute" /> class.
        /// </summary>
        /// <param name="modId">The explicit mod ID override, or null to derive it from the declaring assembly.</param>
        public PMPatchAttribute(string modId = null)
        {
            ModId = modId;
        }
    }
}
