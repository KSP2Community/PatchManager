using JetBrains.Annotations;

namespace PatchManager.SassyPatching.Nodes;

public class Field : Node
{
    public string FieldName;
    [CanBeNull] public Indexer Indexer;
    public Expression FieldValue;
    public Field(Coordinate c, string fieldName, [CanBeNull] Indexer indexer, Expression fieldValue) : base(c)
    {
        FieldName = fieldName;
        Indexer = indexer;
        FieldValue = fieldValue;
    }
}