namespace PatchManager.SassyPatching.Nodes.Attributes;

/// <summary>
/// Represents an attribute applied to a selection block
/// </summary>
public abstract class SelectorAttribute : Node
{
    internal SelectorAttribute(Coordinate c) : base(c)
    {
    }
}