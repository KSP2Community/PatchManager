namespace PatchManager.SassyPatching.Nodes.Indexers;

public class ElementIndexer : Indexer
{
    public string ElementName;

    public ElementIndexer(Coordinate c, string elementName) : base(c)
    {
        ElementName = elementName;
    }
}