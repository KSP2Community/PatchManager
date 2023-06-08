using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes.Statements.FunctionLevel;

public class Return : Node
{
    public Expression ReturnedValue;
    public Return(Coordinate c, Expression returnedValue) : base(c)
    {
        ReturnedValue = returnedValue;
    }
}