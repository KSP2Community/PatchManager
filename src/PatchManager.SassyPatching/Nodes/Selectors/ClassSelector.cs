using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that selects selectables that have a class
/// </summary>
public class ClassSelector : Selector
{
    /// <summary>
    /// The class to select against
    /// </summary>
    public readonly string ClassName;
    internal ClassSelector(Coordinate c, string className) : base(c)
    {
        ClassName = className;
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectableWithEnvironments)
    {
        return selectableWithEnvironments.Where(selectable => selectable.Selectable.MatchesClass(ClassName.Interpolate(selectable.Environment))).ToList();
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