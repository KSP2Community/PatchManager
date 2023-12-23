using JetBrains.Annotations;

namespace PatchManager.SassyPatching.Attributes;

/// <summary>
/// Represents a builtin (C#) library for the sassy patcher execution engine
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class SassyLibraryAttribute : Attribute
{
    /// <summary>
    /// The mod id of the library, this is the part before the ":" in an @use, doesn't have to correspond to the mod guid of the mod defining this library
    /// </summary>
    public string Mod;
    /// <summary>
    /// The actual name of the library, this is the part after the colon
    /// </summary>
    public string Library;

    /// <summary>
    /// Define this as a builtin library
    /// </summary>
    /// <param name="mod">The mod id of the library</param>
    /// <param name="library">The actual name of the library, this is the part after the colon</param>
    public SassyLibraryAttribute(string mod, string library)
    {
        Mod = mod;
        Library = library;
    }
}