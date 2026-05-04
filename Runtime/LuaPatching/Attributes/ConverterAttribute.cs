using System;

namespace PatchManager.LuaPatching.Attributes
{
    /// <summary>
    /// The new equivalent of a "ruleset", this instead represents something that just converts to and from
    /// Just using a ruleset name
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConverterAttribute : Attribute
    {
        /// <summary>
        /// The name of the conversion ruleset
        /// </summary>
        public readonly string Name;
        
        /// <summary>
        /// Create the attribute
        /// </summary>
        /// <param name="name">The name of the conversion ruleset</param>
        public ConverterAttribute(string name)
        {
            Name = name;
        }
    }
}