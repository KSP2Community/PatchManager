using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

public class SetValue : Node
{
    public Expression Value;
    public SetValue(Coordinate c, Expression value) : base(c)
    {
        Value = value;
    }
}