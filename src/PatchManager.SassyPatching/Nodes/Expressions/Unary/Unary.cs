using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

/// <summary>
/// Represents a unary expression (an expression with one child)
/// </summary>
public abstract class Unary : Expression
{
    /// <summary>
    /// The child of this expression
    /// </summary>
    public readonly Expression Child;

    
    
    internal Unary(Coordinate c, Expression child) : base(c)
    {
        Child = child;
    }
    
    internal abstract DataValue GetResult(DataValue child);

    /// <inheritdoc />
    public override DataValue Compute(Environment environment)
    {
        return GetResult(Child.Compute(environment));
    }
}