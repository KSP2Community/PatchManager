namespace PatchManager.SassyPatching.Tests.Validators.Selectors;

public class WithoutClassSelectorValidator : ParseValidator<WithoutClassSelector>
{
    public string ClassName = "";
    public override bool Validate(WithoutClassSelector node) => node.ClassName == ClassName;
}