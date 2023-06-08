namespace PatchManager.SassyPatching.Tests.Validators.Selectors;

public class WithoutNameSelectorValidator : ParseValidator<WithoutNameSelector>
{
    public string NamePattern = "";
    public override bool Validate(WithoutNameSelector node) => node.NamePattern == NamePattern;
}