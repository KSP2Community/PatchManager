using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Execution;

internal class SassyPatchClosure : PatchFunction
{
    public Environment Snapshot;
    public Closure Function;

    public SassyPatchClosure(Environment snapshot, Closure function)
    {
        Snapshot = snapshot;
        Function = function;
    }

    public override DataValue Execute(Environment env, List<PatchArgument> arguments)
    {
        Environment subEnvironment = new Environment(Snapshot.GlobalEnvironment, Snapshot);
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
        try
        {
            foreach (var body in Function.Body)
            {
                body.ExecuteIn(subEnvironment);
            }
        }
        catch (FunctionReturnException ret)
        {
            return ret.FunctionResult;
        }

        return new DataValue(DataValue.DataType.None);
    }
}