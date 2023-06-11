using PatchManager.SassyPatching.Exceptions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents a reference to a variable within the current environment
/// </summary>
public class VariableReference : Expression
{
    /// <summary>
    /// The variable being referenced
    /// </summary>
    public readonly string VariableName;
    internal VariableReference(Coordinate c, string variableName) : base(c)
    {
        VariableName = variableName;
    }

    /// <inheritdoc />
    public override DataValue Compute(Environment environment)
    {
        try
        {
            return environment[VariableName];
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
            throw new InvalidVariableReferenceException(Coordinate,
                $"${VariableName} does not exist in the current scope");
        }
    }
}