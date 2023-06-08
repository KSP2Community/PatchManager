namespace PatchManager.SassyPatching.Nodes.Indexers;

public class ClassIndexer : Indexer
{
    public string ClassName;
    public ClassIndexer(Coordinate c, string className) : base(c)
    {
        ClassName = className;
    }
}