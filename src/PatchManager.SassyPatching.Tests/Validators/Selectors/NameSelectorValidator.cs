namespace PatchManager.SassyPatching.Tests.Validators.Selectors;

public class NameSelectorValidator : ParseValidator<NameSelector>
{
    public string NamePattern = "";
    public override bool Validate(NameSelector node) => node.NamePattern == NamePattern;
}