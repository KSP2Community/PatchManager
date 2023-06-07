namespace PatchManager.SassyPatching.Nodes;

public class SetValue : Node
{
    public Expression Value;
    public SetValue(Coordinate c, Expression value) : base(c)
    {
        Value = value;
    }
}