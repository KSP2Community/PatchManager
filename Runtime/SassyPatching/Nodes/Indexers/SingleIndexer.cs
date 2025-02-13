using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes.Indexers
{
    public class SingleIndexer : Indexer
    {
        public SingleIndexer(Coordinate c, Expression index) : base(c)
        {
            Index = index;
        }
        public Expression Index { get; }
    }
}