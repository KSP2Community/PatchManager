namespace PatchManager.SassyPatching.Tests.Validators.Statements.SelectionLevel;

public class SetValueValidator : ParseValidator<SetValue>
{
    public ParseValidator Value;
    public override bool Validate(SetValue node) => Value.Validate(node.Value);
}