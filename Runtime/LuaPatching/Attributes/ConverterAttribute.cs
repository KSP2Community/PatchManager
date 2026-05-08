using System;

namespace PatchManager.LuaPatching.Attributes
{
    /// <summary>
    /// Marks an <see cref="IConverter" /> implementation as discoverable by name.
    /// </summary>
    /// <remarks>
    /// The decorated type must implement <see cref="IConverter" /> and expose a parameterless constructor;
    /// types missing the interface are skipped with a warning at universe-init time. The created instance is
    /// registered in <see cref="Universe.Converters" /> under <see cref="Name" /> and can then be referenced
    /// from Lua patches by that name (for example <c>PM:PatchAll(script, "JSON", ...)</c>).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConverterAttribute : Attribute
    {
        /// <summary>
        /// The name the converter is registered under in <see cref="Universe.Converters" />.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Creates the attribute with the given converter name.
        /// </summary>
        /// <param name="name">The name the converter is registered under.</param>
        public ConverterAttribute(string name)
        {
            Name = name;
        }
    }
}
