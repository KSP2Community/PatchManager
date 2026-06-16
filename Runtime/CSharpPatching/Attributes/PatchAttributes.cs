using System;

namespace PatchManager.CSharpPatching.Attributes
{
    /// <summary>
    /// Base for the per-domain patch-target attributes. Each subclass bakes the converter name and addressables
    /// label so patch authors never type those strings. Concrete attributes ship with their domains.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class PatchAttribute : Attribute
    {
        /// <summary>The converter to use, as registered via ConverterAttribute.</summary>
        public abstract string Converter { get; }

        /// <summary>The addressables label whose assets this patch targets.</summary>
        public abstract string Label { get; }
    }
}
