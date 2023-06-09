namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that selects children of a selectable that match a selector
/// </summary>
public class ChildSelector : Selector
{
    /// <summary>
    /// The selector to get the parent selectables
    /// </summary>
    public readonly Selector Parent;
    /// <summary>
    /// The selector to match children with
    /// </summary>
    public readonly Selector Child;
    internal ChildSelector(Coordinate c, Selector parent, Selector child) : base(c)
    {
        Parent = parent;
        Child = child;
    }
}