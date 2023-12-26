using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes;

/// <summary>
/// The top level node of a patch file, represents the entire file
/// </summary>
public class SassyPatch : Node
{
    /// <summary>
    /// The list of statements in this patch
    /// </summary>
    public readonly List<Node> Children;

    internal SassyPatch(Coordinate c, List<Node> children) : base(c)
    {
        Children = children;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        foreach (var child in Children)
        {
            child.ExecuteIn(environment);
        }
    }
}