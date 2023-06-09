namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that defines the ruleset that following selectors follow
/// </summary>
public class RulesetSelector : Selector
{
    /// <summary>
    /// The name of the ruleset
    /// </summary>
    public readonly string RulesetName;
    internal RulesetSelector(Coordinate c, string rulesetName) : base(c)
    {
        RulesetName = rulesetName;
    }
}