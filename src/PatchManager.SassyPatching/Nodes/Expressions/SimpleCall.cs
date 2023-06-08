namespace PatchManager.SassyPatching.Nodes.Expressions;

public class SimpleCall : Expression
{
    public string FunctionName;
    public List<CallArgument> Arguments;

    public SimpleCall(Coordinate c, string functionName, List<CallArgument> arguments) : base(c)
    {
        FunctionName = functionName;
        Arguments = arguments;
    }
}