using JetBrains.Annotations;
using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes;

public class CallArgument : Node
{
    [CanBeNull] public string ArgumentName;
    public Expression ArgumentValue;

    public CallArgument(Coordinate c, [CanBeNull] string argumentName, Expression argumentValue) : base(c)
    {
        ArgumentName = argumentName;
        ArgumentValue = argumentValue;
    }

    public CallArgument(Coordinate c, Expression argumentValue) : base(c)
    {
        ArgumentName = null;
        ArgumentValue = argumentValue;
    }
}