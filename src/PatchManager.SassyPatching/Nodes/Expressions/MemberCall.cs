namespace PatchManager.SassyPatching.Nodes.Expressions;

public class MemberCall : Expression
{
    public Expression LeftHandSide;
    public string FunctionName;
    public List<CallArgument> Arguments;

    public MemberCall(Coordinate c, Expression leftHandSide, string functionName, List<CallArgument> arguments) : base(c)
    {
        LeftHandSide = leftHandSide;
        FunctionName = functionName;
        Arguments = arguments;
    }

    public override Value Compute(Environment environment)
    {
        throw new NotImplementedException();
    }
}