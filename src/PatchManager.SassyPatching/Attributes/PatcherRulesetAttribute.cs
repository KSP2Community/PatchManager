namespace PatchManager.SassyPatching.Attributes;



/// <summary>
/// Used to define that a class will be used in the patcher to "get" a ruleset
/// For example [PatcherRuleset("parts")] creates the ":parts" ruleset
/// </summary>

[AttributeUsage(AttributeTargets.Class)]
public class PatcherRulesetAttribute : Attribute
{
    /// <summary>
    /// The name of the ruleset
    /// </summary>
    public string RulesetName;

    /// <summary>
    /// Used to define that a class will be used in the patcher to "get" a ruleset
    /// </summary>
    /// <param name="rulesetName">The name of the ruleset</param>
    public PatcherRulesetAttribute(string rulesetName)
    {
        RulesetName = rulesetName;
    }
}