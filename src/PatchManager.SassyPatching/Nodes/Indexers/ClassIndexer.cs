namespace PatchManager.SassyPatching.Nodes.Indexers;

/// <summary>
/// Represents a field indexer that indexes the first sub-value w/ the specified class
/// </summary>
public class ClassIndexer : Indexer
{
    /// <summary>
    /// The class to index by
    /// </summary>
    public readonly string ClassName;
    internal ClassIndexer(Coordinate c, string className) : base(c)
    {
        ClassName = className;
    }
}