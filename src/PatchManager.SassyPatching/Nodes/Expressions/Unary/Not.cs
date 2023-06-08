namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

public class Not : Unary
{
    public Not(Coordinate c, Expression child) : base(c, child)
    {
    }

    public override Value GetResult(Value child) => child.Truthy;
}