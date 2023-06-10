using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using Environment = PatchManager.SassyPatching.Execution.Environment;

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
    public override DataValue Compute(Environment environment)
    {
        var lhs = LeftHandSide.Compute(environment);
        var lhsType = lhs.Type.ToString().ToLowerInvariant();
        var args = new List<PatchArgument>
        {
            new PatchArgument
            {
                ArgumentName = null,
                ArgumentDataValue = lhs
            }
        };
        args.AddRange(Arguments.Select(x => x.Compute(environment)));
        if (environment.GlobalEnvironment.AllFunctions.TryGetValue($"{lhsType}.{FunctionName}", out var overloadedFunction))
        {
            try
            {
                return overloadedFunction.Execute(environment,args);
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
        if (environment.GlobalEnvironment.AllFunctions.TryGetValue(FunctionName, out var function))
        {
            try
            {
                return function.Execute(environment,args);
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

        throw new InterpreterException(Coordinate, $"Attempting to call {FunctionName}/{lhsType}.{FunctionName} which does not exist");
    }
}