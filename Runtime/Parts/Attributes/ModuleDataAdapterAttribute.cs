using System;

namespace PatchManager.Parts.Attributes
{
    /// <summary>
    /// Marks a class as a typed adapter for one or more part-module data types, surfacing them in Lua under a
    /// strongly-typed wrapper instead of the raw JSON.
    /// </summary>
    /// <remarks>
    /// The decorated class must expose a constructor taking the data entry's <c>JObject</c>. Discovered at
    /// static-init time and registered against each declared <see cref="ValidTypes" /> entry via
    /// <c>PartsUtilities.ModuleDataAdapters</c>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleDataAdapterAttribute : Attribute
    {
        /// <summary>
        /// The data types this adapter is used for.
        /// </summary>
        public readonly Type[] ValidTypes;

        /// <summary>
        /// Creates the attribute with the given list of adapted types.
        /// </summary>
        /// <param name="validTypes">The data types this adapter wraps.</param>
        public ModuleDataAdapterAttribute(params Type[] validTypes) => ValidTypes = validTypes;
    }
}
