using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes.Indexers;

public class SingleIndexer(Coordinate c, Expression index) : Indexer(c)
{
    public Expression Index => index;
}