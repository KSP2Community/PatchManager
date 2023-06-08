namespace PatchManager.SassyPatching.Nodes.Indexers;

public class NumberIndexer : Indexer
{
    public ulong Index;
    public NumberIndexer(Coordinate c, ulong index) : base(c)
    {
        Index = index;
    }
}