namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

public class Positive : Unary
{
    public Positive(Coordinate c, Expression child) : base(c, child)
    {
    }

    public override Value GetResult(Value child)
    {
        return child;
    }
}