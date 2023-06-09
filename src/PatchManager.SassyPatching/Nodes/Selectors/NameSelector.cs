namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that matches selectables that have a name that matches a name pattern
/// </summary>
public class NameSelector : Selector
{    
    /// <summary>
    /// The name pattern to match against, can contain */? wildcards
    /// </summary>
    public string NamePattern;
    internal NameSelector(Coordinate c, string namePattern) : base(c)
    {
        NamePattern = namePattern;
    }
}