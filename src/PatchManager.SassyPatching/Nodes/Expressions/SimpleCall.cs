using PatchManager.SassyPatching.Exceptions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

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
    public override DataValue Compute(Environment environment)
    {
        if (environment.GlobalEnvironment.AllFunctions.TryGetValue(FunctionName, out var function))
        {
            try
            {
                return function.Execute(environment, Arguments.Select(x => x.Compute(environment)).ToList());
            }
            catch (InterpreterException i)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InterpreterException(Coordinate, e.ToString());
            }
        }

        throw new InterpreterException(Coordinate, $"Attempting to call {FunctionName} which does not exist");
    }
}