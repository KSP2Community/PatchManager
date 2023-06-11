using JetBrains.Annotations;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
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
        Environment subEnvironment = new Environment(env.GlobalEnvironment, env);
        foreach (var arg in Function.Arguments)
        {
            // As per usual we consume
            bool foundPositional = false;
            DataValue argument = null;
            int removalIndex = -1;
            for (int i = 0; i < arguments.Count; i++)
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
}