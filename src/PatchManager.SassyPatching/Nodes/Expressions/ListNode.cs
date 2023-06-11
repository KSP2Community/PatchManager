using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents a list initializer
/// </summary>
public class ListNode : Expression
{
    /// <summary>
    /// The list of values contained within this list
    /// </summary>
    public readonly List<Expression> Expressions;
    internal ListNode(Coordinate c, List<Expression> expressions) : base(c)
    {
        Expressions = expressions;
    }

    /// <inheritdoc />
    public override DataValue Compute(Environment environment)
    {
        return Expressions.Select(x => x.Compute(environment)).ToList();
    }
}