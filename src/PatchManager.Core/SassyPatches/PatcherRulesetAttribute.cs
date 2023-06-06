namespace PatchManager.Core.SassyPatches;


[AttributeUsage(AttributeTargets.Class)]
public class PatcherRulesetAttribute : Attribute
{
    public string RulesetName;

    public PatcherRulesetAttribute(string rulesetName)
    {
        RulesetName = rulesetName;
    }
}