namespace PatchManager.SassyPatching.Nodes.Indexers;

/// <summary>
/// Represents a field indexer that indexes by element type
/// </summary>
public class ElementIndexer : Indexer
{
    /// <summary>
    /// The element type to index by
    /// </summary>
    public readonly string ElementName;

    internal ElementIndexer(Coordinate c, string elementName) : base(c)
    {
        ElementName = elementName;
    }
}