using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

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
    public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectableWithEnvironments)
    {
        return new();
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAllTopLevel(string type, string name, string data, Environment baseEnvironment, out ISelectable rulesetMatchingObject)
    {
        if (!Universe.RuleSets.TryGetValue(RulesetName, out var ruleSet))
        {
            throw new InterpreterException(Coordinate, $"Ruleset: {RulesetName} does not exist!");
        }

        if (ruleSet.Matches(type))
        {
            rulesetMatchingObject = ruleSet.ConvertToSelectable(type, name,data);
            if (rulesetMatchingObject != null)
            {
                return
                [
                    new SelectableWithEnvironment
                    {
                        Selectable = rulesetMatchingObject,
                        Environment = new Environment(baseEnvironment.GlobalEnvironment, baseEnvironment)
                    }
                ];
            }
        }
        rulesetMatchingObject = null;
        return [];

    }

    public override List<SelectableWithEnvironment> CreateNew(List<DataValue> rulesetArguments, Environment baseEnvironment, out INewAsset newAsset)
    {
        if (!Universe.RuleSets.TryGetValue(RulesetName, out var ruleSet))
        {
            throw new InterpreterException(Coordinate, $"Ruleset: {RulesetName} does not exist!");
        }

        var newObject = ruleSet.CreateNew(rulesetArguments);
        newAsset = newObject;
        return
        [
            new SelectableWithEnvironment
            {
                Selectable = newAsset.Selectable,
                Environment = new Environment(baseEnvironment.GlobalEnvironment, baseEnvironment)
            }
        ];

    }
}