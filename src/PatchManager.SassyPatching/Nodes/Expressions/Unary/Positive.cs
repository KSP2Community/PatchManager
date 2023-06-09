namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

/// <summary>
/// Represents a positive expression which returns its child
/// </summary>
public class Positive : Unary
{
    internal Positive(Coordinate c, Expression child) : base(c, child)
    {
    }

    internal override Value GetResult(Value child)
    {
        return child;
    }
}