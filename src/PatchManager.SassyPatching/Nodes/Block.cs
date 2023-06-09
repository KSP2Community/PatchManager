namespace PatchManager.SassyPatching.Nodes;

/// <summary>
/// Represents a group of nodes
/// </summary>
public class Block : Node
{
    /// <summary>
    /// The children of this block node
    /// </summary>
    public readonly List<Node> Children;
    
    internal Block(Coordinate c, List<Node> children) : base(c)
    {
        Children = children;
    }
}