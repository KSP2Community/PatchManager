namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents a member call, or a call syntax where the left hand side is the first argument
/// </summary>
public class MemberCall : Expression
{
    /// <summary>
    /// The object getting member called
    /// </summary>
    public readonly Expression LeftHandSide;
    /// <summary>
    /// The function being called, first tries to find a function named [LeftHandSide Type].name then name
    /// </summary>
    public readonly string FunctionName;
    /// <summary>
    /// The arguments being passed to the function, other than the left hand side
    /// </summary>
    public readonly List<CallArgument> Arguments;

    internal MemberCall(Coordinate c, Expression leftHandSide, string functionName, List<CallArgument> arguments) : base(c)
    {
        LeftHandSide = leftHandSide;
        FunctionName = functionName;
        Arguments = arguments;
    }

    /// <inheritdoc />
    public override Value Compute(Environment environment)
    {
        throw new NotImplementedException();
    }
}