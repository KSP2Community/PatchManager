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
    public override List<ISelectable> SelectAllTopLevel(string type, string name, string data, out ISelectable rulesetMatchingObject)
    {
        if (Universe.RuleSets.TryGetValue(RulesetName, out var ruleSet))
        {
            if (ruleSet.Matches(type))
            {
                rulesetMatchingObject = ruleSet.ConvertToSelectable(type, name,data);
                return new List<ISelectable>
                {
                    rulesetMatchingObject
                };
            }
            else
            {
                rulesetMatchingObject = null;
                return new();
            }
        }
        else
        {
            throw new InterpreterException(Coordinate, $"Ruleset: {RulesetName} does not exist!");
        }
    }

    public override List<ISelectable> CreateNew(List<DataValue> rulesetArguments, out INewAsset newAsset)
    {
        if (Universe.RuleSets.TryGetValue(RulesetName, out var ruleSet))
        {
            var newObject = ruleSet.CreateNew(rulesetArguments);
            newAsset = newObject;
            return new List<ISelectable> { newObject.Selectable };
        }
        else
        {
            throw new InterpreterException(Coordinate, $"Ruleset: {RulesetName} does not exist!");
        }
    }
}