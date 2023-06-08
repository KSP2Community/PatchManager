namespace PatchManager.SassyPatching.Tests.Validators.Statements;

public class VarDeclValidator : ParseValidator<VariableDeclaration>
{
    public string Variable = "";
    public ParseValidator Value = new FalseValidator();
    public override bool Validate(VariableDeclaration variableDeclaration)
    {
        if (variableDeclaration.Variable != Variable) return false;
        return Value.Validate(variableDeclaration.Value);
    }
}