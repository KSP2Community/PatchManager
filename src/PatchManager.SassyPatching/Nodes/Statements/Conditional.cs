using JetBrains.Annotations;

namespace PatchManager.SassyPatching.Nodes.Statements;

/// <summary>
/// Represents a conditional (if-else) statement
/// </summary>
public class Conditional : Node
{
    /// <summary>
    /// The condition that is being tested
    /// </summary>
    public readonly Node Condition;
    /// <summary>
    /// The list of nodes to be executed upon a truthy result
    /// </summary>
    public readonly List<Node> Body;
    /// <summary>
    /// The node to be executed upon a falsy result, may not exist
    /// </summary>
    [CanBeNull] public readonly Node Else;

    internal Conditional(Coordinate c, Node condition, List<Node> body, [CanBeNull] Node @else = null) : base(c)
    {
        Condition = condition;
        Body = body;
        Else = @else;
    }
}