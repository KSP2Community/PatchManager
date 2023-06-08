namespace PatchManager.SassyPatching.Tests.Validators.Statements.SelectionLevel;

public class MergeValueValidator : ParseValidator<MergeValue>
{
    public ParseValidator Value;
    public override bool Validate(MergeValue node) => Value.Validate(node.Value);
}