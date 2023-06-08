namespace PatchManager.SassyPatching.Nodes.Expressions;

public abstract class Expression : Node
{

    public abstract Value Compute(Environment environment);
    protected Expression(Coordinate c) : base(c)
    {
    }
}