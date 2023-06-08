namespace PatchManager.SassyPatching.Nodes.Expressions;

public abstract class Expression : Node
{
    protected Expression(Coordinate c) : base(c)
    {
    }
}