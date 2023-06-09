namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents a simple function call
/// </summary>
public class SimpleCall : Expression
{
    /// <summary>
    /// The function being called
    /// </summary>
    public readonly string FunctionName;
    /// <summary>
    /// The arguments being passed to the function call
    /// </summary>
    public readonly List<CallArgument> Arguments;

    internal SimpleCall(Coordinate c, string functionName, List<CallArgument> arguments) : base(c)
    {
        FunctionName = functionName;
        Arguments = arguments;
    }

    /// <inheritdoc />
    public override Value Compute(Environment environment)
    {
        throw new NotImplementedException();
    }
}