namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

public abstract class Unary : Expression
{
    public Expression Child;

    protected Unary(Coordinate c, Expression child) : base(c)
    {
        Child = child;
    }
}