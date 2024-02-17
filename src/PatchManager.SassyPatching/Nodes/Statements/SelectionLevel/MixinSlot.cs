using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

public class MixinSlot(Coordinate c) : Node(c), ISelectionAction
{
    public override void ExecuteIn(Environment environment)
    {
    }

    public void ExecuteOn(Environment environment, ISelectable selectable, IModifiable modifiable)
    {
        var actions = environment.SlotActions;
        if (actions == null)
        {
            throw new InterpreterException(Coordinate, "Attempting to insert into a mixin slot without there being any actions passed for this purpose");
        }

        foreach (var action in actions)
        {
            if (action is ISelectionAction selectionAction)
            {
                selectionAction.ExecuteOn(environment, selectable, modifiable);
            }
            else
            {
                action.ExecuteIn(environment);
            }
        }
    }
}