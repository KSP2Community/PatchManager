namespace PatchManager.SassyPatching.Nodes.Indexers;

/// <summary>
/// Represents a field indexer that indexes by element type (same as <see cref="ElementIndexer"/>)
/// </summary>
public class StringIndexer : Indexer
{
    /// <summary>
    /// The element type to index by
    /// </summary>
    public readonly string Index;
    internal StringIndexer(Coordinate c, string index) : base(c) => Index = index;
}