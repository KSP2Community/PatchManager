using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// Represents a deletion selection action
/// </summary>
public class DeleteValue : Node, ISelectionAction
{
    internal DeleteValue(Coordinate c) : base(c)
    {
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
            throw new InterpreterException(Coordinate, "Attempting to delete an unmodifiable selection");
        }
        Console.WriteLine($"Deleting modifiable of type {modifiable}");
        modifiable.Set(new DataValue(DataValue.DataType.Deletion));
    }
}