using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;

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

    /// <inheritdoc />
    public override List<ISelectable> SelectAll(List<ISelectable> selectables)
    {
        return new();
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAllTopLevel(string type, string data)
    {
        if (Universe.RuleSets.TryGetValue(RulesetName, out var ruleSet))
        {
            if (ruleSet.Matches(type))
            {
                return new List<ISelectable>
                {
                    ruleSet.ConvertToSelectable(data)
                };
            }
            else
            {
                return new();
            }
        }
        else
        {
            throw new InterpreterException(Coordinate, $"Ruleset: {RulesetName} does not exist!");
        }
    }
}