namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

/// <summary>
/// Represents a not operation which returns the opposite of the truthiness  of its child
/// </summary>
/// <seealso cref="Value.Truthy"/>
public class Not : Unary
{
    internal Not(Coordinate c, Expression child) : base(c, child)
    {
    }

    internal override Value GetResult(Value child) => !child.Truthy;
}