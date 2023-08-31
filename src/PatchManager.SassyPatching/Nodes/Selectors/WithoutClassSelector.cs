using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that matches selectables that don't have the class defined in this
/// </summary>
public class WithoutClassSelector : Selector
{
    /// <summary>
    /// The class to match against
    /// </summary>
    public readonly string ClassName;
    internal WithoutClassSelector(Coordinate c, string className) : base(c)
    {
        ClassName = className;
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAll(List<ISelectable> selectables)
    {
        return selectables.Where(selectable => !selectable.MatchesClass(ClassName)).ToList();
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAllTopLevel(string type,string name, string data, out ISelectable rulesetMatchingObject)
    {
        rulesetMatchingObject = null;
        return new();
    }

    public override List<ISelectable> CreateNew(List<DataValue> rulesetArguments, out INewAsset newAsset)
    {
        newAsset = null;
        return new();
    }
}