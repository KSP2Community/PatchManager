using System;

namespace PatchManager.Parts.Attributes
{
    /// <summary>
    /// Types that take this attribute must have a constructor that takes the following arguments
    /// JObject - ModuleData.DataObject
    /// ModuleUserData - Module
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleDataAdapterAttribute : Attribute
    {
        /// <summary>
        /// The types this adapter is used for
        /// </summary>
        public readonly Type[] ValidTypes;
        /// <summary>
        /// Mark this class as a custom module data adapter
        /// </summary>
        /// <param name="validTypes">What data types it adapts</param>
        public ModuleDataAdapterAttribute(params Type[] validTypes) => ValidTypes = validTypes;
    }
}