namespace PatchManager.SassyPatching;



/// <summary>
/// Used to define that a class will be used in the patcher to "get" a ruleset
/// For example [PatcherRuleset("parts")] creates the ":parts" ruleset
/// </summary>

[AttributeUsage(AttributeTargets.Class)]
public class PatcherRulesetAttribute : Attribute
{
    public string RulesetName;

    public PatcherRulesetAttribute(string rulesetName)
    {
        RulesetName = rulesetName;
    }
}