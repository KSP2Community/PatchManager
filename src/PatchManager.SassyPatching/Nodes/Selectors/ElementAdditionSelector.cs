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
}