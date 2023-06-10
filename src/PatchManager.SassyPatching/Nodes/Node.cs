using Environment = PatchManager.SassyPatching.Execution.Environment;

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
    internal Node(Coordinate c)
    {
        Coordinate = c;
    }

    /// <summary>
    /// Execute this node in the given environment
    /// </summary>
    /// <param name="environment">The given environment</param>
    public abstract void ExecuteIn(Environment environment);
}