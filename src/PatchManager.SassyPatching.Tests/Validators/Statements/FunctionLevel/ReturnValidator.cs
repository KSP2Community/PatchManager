namespace PatchManager.SassyPatching.Tests.Validators.Statements.FunctionLevel;

public class ReturnValidator : ParseValidator<Return>
{
    public ParseValidator ReturnedValue = new FalseValidator();
    public override bool Validate(Return node)
    {
        return ReturnedValue.Validate(node.ReturnedValue);
    }
}