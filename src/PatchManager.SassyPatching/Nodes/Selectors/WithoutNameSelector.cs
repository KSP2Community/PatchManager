using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that matches selectables that don't have a name that matches a name pattern
/// </summary>
public class WithoutNameSelector : Selector
{
    /// <summary>
    /// The name pattern to match against, can contain */? wildcards
    /// </summary>
    public readonly string NamePattern;
    internal WithoutNameSelector(Coordinate c, string namePattern) : base(c)
    {
        NamePattern = namePattern;
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectableWithEnvironments)
    {
        return selectableWithEnvironments.Where(selectable => !selectable.Selectable.MatchesName(NamePattern.Interpolate(selectable.Environment))).ToList();
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAllTopLevel(string type, string name, string data, Environment baseEnvironment, out ISelectable rulesetMatchingObject)
    {
        rulesetMatchingObject = null;
        return new();
    }
    
    public override List<SelectableWithEnvironment> CreateNew(List<DataValue> rulesetArguments, Environment baseEnvironment, out INewAsset newAsset)
    {
        newAsset = null;
        return new();
    }
}