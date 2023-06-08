namespace PatchManager.SassyPatching.Tests.Validators.Selectors;

public class ChildSelectorValidator : ParseValidator<ChildSelector>
{
    public ParseValidator Parent = new FalseValidator();
    public ParseValidator Child = new FalseValidator();
    public override bool Validate(ChildSelector node) => Parent.Validate(node.Parent) && Child.Validate(node.Child);
}