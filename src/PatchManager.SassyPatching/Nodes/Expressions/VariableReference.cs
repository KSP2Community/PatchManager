namespace PatchManager.SassyPatching.Nodes.Expressions;

public class VariableReference : Expression
{
    public string VariableName;
    public VariableReference(Coordinate c, string variableName) : base(c)
    {
        VariableName = variableName;
    }
}