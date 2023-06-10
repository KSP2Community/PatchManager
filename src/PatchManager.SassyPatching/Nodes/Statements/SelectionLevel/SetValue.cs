using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// Represents a value setting selection action
/// </summary>
public class SetValue : Node, ISelectionAction
{
    /// <summary>
    /// The value to set the selection to
    /// </summary>
    public readonly Expression Value;
    internal SetValue(Coordinate c, Expression value) : base(c)
    {
        Value = value;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void ExecuteOn(Environment environment, ISelectable selectable, IModifiable modifiable)
    {
        if (modifiable == null)
        {
            throw new InterpreterException(Coordinate, "Attempting to set a non modifiable selection");
        }

        var value = modifiable.Get();
        var subEnvironment = new Environment(environment.GlobalEnvironment, environment)
        {
            ["value"] = value
        };
        var result = Value.Compute(subEnvironment);
        modifiable.Set(result);
    }
}