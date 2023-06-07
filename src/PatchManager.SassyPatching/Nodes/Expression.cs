namespace PatchManager.SassyPatching.Nodes;

public abstract class Expression : Node
{
    protected Expression(Coordinate c) : base(c)
    {
    }
}