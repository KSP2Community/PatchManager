using PatchManager.Parts.SassyPatching.Selectables;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.Parts.SassyPatching.Rulesets;

/// <summary>
/// The `:parts` ruleset used by sassy patching
/// </summary>
[PatcherRuleset("parts")]
public class PartsRuleset : IPatcherRuleSet
{
    /// <summary>
    /// This rule set matches the "part" type used by the core engine for patching
    /// </summary>
    public string PatchTypeMatch => "part_data";
    /// <summary>
    /// Converts the part json to an ISelectable following this ruleset
    /// </summary>
    /// <param name="jsonData">The part json</param>
    /// <returns>An ISelectable that follows the part ruleset</returns>
    public ISelectable ConvertToSelectable(string jsonData)
    {
        return new PartSelectable(jsonData);
    }
}