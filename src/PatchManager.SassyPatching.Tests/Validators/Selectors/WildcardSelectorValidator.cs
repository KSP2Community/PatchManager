namespace PatchManager.SassyPatching.Tests.Validators.Selectors;

public class WildcardSelectorValidator : ParseValidator<WildcardSelector>
{
    public override bool Validate(WildcardSelector node) => true;
}