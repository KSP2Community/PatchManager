using JetBrains.Annotations;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes;
using PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;
using PatchManager.SassyPatching.Nodes.Statements.TopLevel;

namespace PatchManager.SassyPatching.Execution;

public class PatchMixin
{
    public Mixin Function;

    public PatchMixin(Mixin mixin)
    {
        Function = mixin;
    }

    public void Include(Environment env, List<PatchArgument> arguments, ISelectable selectable, [CanBeNull] IModifiable modifiable)
    {
        var subEnvironment = new Environment(env.GlobalEnvironment, env);
        foreach (var arg in Function.Arguments)
        {
            ConsumeMixinArgument(arguments, arg, subEnvironment);
        }

        if (arguments.Count > 0)
        {
            throw new InvocationException("Too many arguments have been passed");
        }
        foreach (var body in Function.Body)
        {
            if (body is ISelectionAction selectionAction)
            {
                selectionAction.ExecuteOn(subEnvironment, selectable, modifiable);
            }
            else
            {
                body.ExecuteIn(subEnvironment);
            }
        }

    }

    private static void ConsumeMixinArgument(List<PatchArgument> arguments, Argument arg, Environment subEnvironment)
    {
        // As per usual we consume
        var foundPositional = false;
        DataValue argument = null;
        var removalIndex = -1;
        for (var i = 0; i < arguments.Count; i++)
        {
            if (!foundPositional && arguments[i].ArgumentName == null)
            {
                foundPositional = true;
                removalIndex = i;
                argument = arguments[i].ArgumentDataValue;
            }

            if (arguments[i].ArgumentName != arg.Name) continue;
            removalIndex = i;
            argument = arguments[i].ArgumentDataValue;
            break;
        }

        if (removalIndex >= 0)
        {
            arguments.RemoveAt(removalIndex);
        }
        if (argument == null)
        {
            if (arg.Value != null)
            {
                subEnvironment[arg.Name] = arg.Value.Compute(subEnvironment);
            }
            else
            {
                throw new InvocationException($"No value found for argument: {arg.Name}");
            }
        }
        else
        {
            // args.Add(CheckParameter(parameter, argument));
            subEnvironment[arg.Name] = argument;
        }
    }
}