using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions;

public class VariableReference : Expression
{
    public string VariableName;
    public VariableReference(Coordinate c, string variableName) : base(c)
    {
        VariableName = variableName;
    }

    public override Value Compute(Environment environment)
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