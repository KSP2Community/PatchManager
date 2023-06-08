namespace PatchManager.SassyPatching.Tests.Validators.Expressions;

public class VariableReferenceValidator : ParseValidator<VariableReference>
{
    public string VariableName = "";
    public override bool Validate(VariableReference node)
    {
        return node.VariableName == VariableName;
    }
}