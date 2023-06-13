using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selection action that matches all selectables
/// </summary>
public class WildcardSelector : Selector
{
    internal WildcardSelector(Coordinate c) : base(c)
    {
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAll(List<ISelectable> selectables)
    {
        return selectables;
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAllTopLevel(string type, string name, string data, out ISelectable rulesetMatchingObject)
    {
        rulesetMatchingObject = null;
        return new();
    }
}