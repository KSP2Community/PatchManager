namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector node
/// </summary>
public abstract class Selector : Node
{
    internal Selector(Coordinate c) : base(c)
    {
    }
}