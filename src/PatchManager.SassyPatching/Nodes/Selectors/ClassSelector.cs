namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that selects selectables that have a class
/// </summary>
public class ClassSelector : Selector
{
    /// <summary>
    /// The class to select against
    /// </summary>
    public readonly string ClassName;
    internal ClassSelector(Coordinate c, string className) : base(c)
    {
        ClassName = className;
    }
}