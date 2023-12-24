using JetBrains.Annotations;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Attributes;



/// <summary>
/// Used to define that a class will be used in the patcher to "get" a ruleset
/// For example [PatcherRuleset("parts")] creates the ":parts" ruleset
/// </summary>

[AttributeUsage(AttributeTargets.Class)]
[BaseTypeRequired(typeof(IPatcherRuleSet))]
[MeansImplicitUse]
public class PatcherRulesetAttribute : Attribute
{
    /// <summary>
    /// The name of the ruleset
    /// </summary>
    public readonly string RulesetName;

    /// <summary>
    /// Labels that should always be loaded, as this ruleset will match them
    /// </summary>
    public string[] PreloadLabels;
    
    /// <summary>
    /// Used to define that a class will be used in the patcher to "get" a ruleset
    /// </summary>
    /// <param name="rulesetName">The name of the ruleset</param>
    /// <param name="preloadLabels">Labels that should always be loaded, as this ruleset will match them</param>
    public PatcherRulesetAttribute(string rulesetName, params string[] preloadLabels)
    {
        RulesetName = rulesetName;
        PreloadLabels = preloadLabels;
    }
}