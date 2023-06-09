namespace PatchManager.SassyPatching.Nodes;

/// <summary>
/// An abstract class representing a node in the tree of a patch file
/// </summary>
public abstract class Node
{
    /// <summary>
    /// The location of this node
    /// </summary>
    public readonly Coordinate Coordinate;

    /// <summary>
    /// Creates a new node at a location
    /// </summary>
    /// <param name="c">The location of the node</param>
    protected Node(Coordinate c)
    {
        Coordinate = c;
    }
}