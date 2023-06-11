using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that selects all selectables that are an element type
/// </summary>
public class ElementSelector : Selector
{
    /// <summary>
    /// The element type to select
    /// </summary>
    public readonly string ElementName;
    internal ElementSelector(Coordinate c, string elementName) : base(c)
    {
        ElementName = elementName;
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAll(List<ISelectable> selectables)
    {
        return selectables.Where(selectable => selectable.MatchesElement(ElementName)).ToList();
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAllTopLevel(string type, string data, out ISelectable rulesetMatchingObject)
    {
        rulesetMatchingObject = null;
        return new();
    }
}