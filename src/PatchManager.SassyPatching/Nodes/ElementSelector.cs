namespace PatchManager.SassyPatching.Nodes;

public class ElementSelector : Selector
{
    public string ElementName;
    public ElementSelector(Coordinate c, string elementName) : base(c)
    {
        ElementName = elementName;
    }
}