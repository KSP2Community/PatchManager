namespace PatchManager.SassyPatching.Tests.Validators.Selectors;

public class RulesetSelectorValidator : ParseValidator<RulesetSelector>
{
    public string RulesetName = "";
    public override bool Validate(RulesetSelector node) => node.RulesetName == RulesetName;
}