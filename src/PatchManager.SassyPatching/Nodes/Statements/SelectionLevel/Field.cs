using JetBrains.Annotations;
using PatchManager.SassyPatching.Nodes.Expressions;
using PatchManager.SassyPatching.Nodes.Indexers;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// Represents a field setting selection action
/// </summary>
public class Field : Node
{
    /// <summary>
    /// The field to be set
    /// </summary>
    public readonly string FieldName;
    /// <summary>
    /// The index into the field, if there is one
    /// </summary>
    [CanBeNull] public readonly Indexer Indexer;
    /// <summary>
    /// The value to set the field to
    /// </summary>
    public readonly Expression FieldValue;
    internal Field(Coordinate c, string fieldName, [CanBeNull] Indexer indexer, Expression fieldValue) : base(c)
    {
        FieldName = fieldName;
        Indexer = indexer;
        FieldValue = fieldValue;
    }
}