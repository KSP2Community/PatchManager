namespace PatchManager.SassyPatching.Nodes.Selectors;

public class ElementAdditionSelector : Selector
{
    public string ElementName;
    public ElementAdditionSelector(Coordinate c, string elementName) : base(c)
    {
        ElementName = elementName;
    }
}