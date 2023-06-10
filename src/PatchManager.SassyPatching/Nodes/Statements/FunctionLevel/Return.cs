using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.FunctionLevel;

/// <summary>
/// Represents a function return statement
/// </summary>
public class Return : Node
{
    /// <summary>
    /// The value to be returned from the function
    /// </summary>
    public readonly Expression ReturnedValue;
    internal Return(Coordinate c, Expression returnedValue) : base(c)
    {
        ReturnedValue = returnedValue;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        throw new FunctionReturnException(ReturnedValue.Compute(environment));
    }
}