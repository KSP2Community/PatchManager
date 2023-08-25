using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that adds an element to a selectable and selects it
/// </summary>
public class ElementAdditionSelector : Selector
{
    /// <summary>
    /// The element type to add to the selectable
    /// </summary>
    public string ElementName;
    internal ElementAdditionSelector(Coordinate c, string elementName) : base(c)
    {
        ElementName = elementName;
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAll(List<ISelectable> selectables)
    {
        return selectables.Select(selectable => selectable.AddElement(ElementName)).ToList();
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAllTopLevel(string type, string name, string data, out ISelectable rulesetMatchingObject)
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