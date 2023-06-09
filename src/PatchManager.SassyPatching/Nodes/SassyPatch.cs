namespace PatchManager.SassyPatching.Nodes;

/// <summary>
/// The top level node of a patch file, represents the entire file
/// </summary>
public class SassyPatch : Node
{
    /// <summary>
    /// 
    /// </summary>
    public readonly List<Node> Children;

    /// <summary>
    /// Create a new top level node
    /// </summary>
    /// <param name="c">The location of the node</param>
    /// <param name="children">The children of the node</param>
    public SassyPatch(Coordinate c, List<Node> children) : base(c)
    {
        Children = children;
    }
}