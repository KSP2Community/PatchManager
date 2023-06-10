using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// Represents a merge selection action
/// </summary>
public class MergeValue : Node, ISelectionAction
{
    /// <summary>
    /// The value the selection is going to be merged with, should evaluate to a dictionary
    /// </summary>
    public readonly Expression Value;
    internal MergeValue(Coordinate c, Expression value) : base(c)
    {
        Value = value;
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
            throw new InterpreterException(Coordinate, "Attempting to merge non modifiable selection");
        }

        var value = modifiable.Get();
        var subEnvironment = new Environment(environment.GlobalEnvironment, environment)
        {
            ["value"] = value
        };
        var toMerge = Value.Compute(subEnvironment);
        if (value.IsDictionary && toMerge.IsDictionary)
        {
            foreach (var kv in toMerge.Dictionary)
            {
                value.Dictionary[kv.Key] = kv.Value;
            }

            modifiable.Set(value);
        }
        else
        {
            throw new BinaryExpressionTypeException(Coordinate, "merge", value.Type.ToString(),
                toMerge.Type.ToString());
        }
    }
}