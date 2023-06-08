namespace PatchManager.SassyPatching.Nodes.Selectors;

public class ElementSelector : Selector
{
    public string ElementName;
    public ElementSelector(Coordinate c, string elementName) : base(c)
    {
        ElementName = elementName;
    }
}