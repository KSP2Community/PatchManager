using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

public class MergeValue : Node
{
    public Expression Value;
    public MergeValue(Coordinate c, Expression value) : base(c)
    {
        Value = value;
    }
}