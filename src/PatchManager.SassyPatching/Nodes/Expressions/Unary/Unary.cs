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
    
    internal abstract Value GetResult(Value child);

    /// <inheritdoc />
    public override Value Compute(Environment environment)
    {
        return GetResult(Child.Compute(environment));
    }
}