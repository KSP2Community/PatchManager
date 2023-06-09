namespace PatchManager.SassyPatching.Nodes.Indexers;

/// <summary>
/// Represents a field indexer that indexes by number
/// </summary>
public class NumberIndexer : Indexer
{
    /// <summary>
    /// The index to index the field by
    /// </summary>
    public readonly ulong Index;
    internal NumberIndexer(Coordinate c, ulong index) : base(c)
    {
        Index = index;
    }
}