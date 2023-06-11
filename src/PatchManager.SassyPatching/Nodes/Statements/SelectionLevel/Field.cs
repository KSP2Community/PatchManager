using JetBrains.Annotations;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes.Expressions;
using PatchManager.SassyPatching.Nodes.Indexers;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// Represents a field setting selection action
/// </summary>
public class Field : Node, ISelectionAction
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

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
    }

    /// <inheritdoc />
    public void ExecuteOn(Environment environment, ISelectable selectable, IModifiable modifiable)
    {
        if (modifiable == null)
        {
            throw new InterpreterException(Coordinate, "Attempting to modify an unmodifiable selection");
        }

        var value = Indexer switch
        {
            ElementIndexer elementIndexer => modifiable.GetFieldByElement(FieldName, elementIndexer.ElementName),
            StringIndexer stringIndexer => modifiable.GetFieldByElement(FieldName, stringIndexer.Index),
            NumberIndexer numberIndexer => modifiable.GetFieldByNumber(FieldName, numberIndexer.Index),
            ClassIndexer classIndexer => modifiable.GetFieldByClass(FieldName, classIndexer.ClassName),
            _ => modifiable.GetFieldValue(FieldName)
        };
        var subEnvironment = new Environment(environment.GlobalEnvironment, environment)
        {
            ["value"] = value
        };
        var result = FieldValue.Compute(subEnvironment);
        switch (Indexer)
        {
            case ElementIndexer setElementIndexer:
                modifiable.SetFieldByElement(FieldName, setElementIndexer.ElementName, result);
                return;
            case StringIndexer setStringIndexer:
                modifiable.SetFieldByElement(FieldName, setStringIndexer.Index, result);
                return;
            case NumberIndexer setNumberIndexer:
                modifiable.SetFieldByNumber(FieldName, setNumberIndexer.Index, result);
                return;
            case ClassIndexer setClassIndexer:
                modifiable.SetFieldByClass(FieldName, setClassIndexer.ClassName, result);
                return;
            default:
                modifiable.SetFieldValue(FieldName, result);
                return;
        }
    }
}