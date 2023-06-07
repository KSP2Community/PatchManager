namespace PatchManager.SassyPatching.Nodes;

public class MergeValue : Node
{
    public Expression Value;
    public MergeValue(Coordinate c, Expression value) : base(c)
    {
        Value = value;
    }
}