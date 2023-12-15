using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Execution;

internal class ManagedPatchFunction : PatchFunction
{
    // private Func<Environment, List<PatchArgument>,Value> _execute;
    private readonly MethodInfo _info;
    [CanBeNull] private object _target;

    public ManagedPatchFunction(MethodInfo info, [CanBeNull] object target = null)
    {
        _info = info;
        _target = target;
    }

    private static object CheckParameter(ParameterInfo info, DataValue argument)
    {
        var obj = argument.To(info.ParameterType);
        return obj;
    }

    public override DataValue Execute(Environment env, List<PatchArgument> arguments)
    {
        List<object> args = new();

        // We are going to consume arguments as we go to prevent errors
        
        foreach (var parameter in _info.GetParameters())
        {
            var argumentName = parameter.Name;
            if (parameter.ParameterType.GetCustomAttribute<SassyNameAttribute>() is { } attribute)
            {
                argumentName = attribute.ArgumentName;
            }
            
            if (parameter.ParameterType == typeof(Environment))
            {
                args.Add(env);
                continue;
            }

            if (parameter.ParameterType == typeof(GlobalEnvironment))
            {
                args.Add(env.GlobalEnvironment);
                continue;
            }

            if (parameter.ParameterType == typeof(Universe))
            {
                args.Add(env.GlobalEnvironment.Universe);
                continue;
            }

            if (parameter.ParameterType == typeof(List<PatchArgument>))
            {
                args.Add(new List<PatchArgument>(arguments));
                arguments.Clear();
                continue;
            }
            
            if (parameter.ParameterType == typeof(List<DataValue>) &&
                parameter.GetCustomAttributes().OfType<VarArgsAttribute>().Any())
            {
                var varArgs = new List<DataValue>();
                var remove = new List<int>();
                for (var i = 0; i < arguments.Count; i++)
                {
                    if (arguments[i].ArgumentName != null) continue;
                    varArgs.Add(arguments[i].ArgumentDataValue);
                    remove.Add(i);
                }

                args.Add(varArgs);
                for (var i = remove.Count - 1; i >= 0; i--)
                {
                    arguments.RemoveAt(i);
                }
                continue;
            }

            if (parameter.ParameterType == typeof(Dictionary<string, DataValue>) &&
                parameter.GetCustomAttributes().OfType<VarArgsAttribute>().Any())
            {
                var varArgs = new Dictionary<string, DataValue>();
                var remove = new List<int>();
                for (var i = 0; i < arguments.Count; i++)
                {
                    if (arguments[i].ArgumentName == null) continue;
                    varArgs.Add(arguments[i].ArgumentName, arguments[i].ArgumentDataValue);
                    remove.Add(i);
                }

                args.Add(varArgs);
                for (var i = remove.Count - 1; i >= 0; i--)
                {
                    arguments.RemoveAt(i);
                }
                continue;
            }

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

                if (arguments[i].ArgumentName != argumentName) continue;
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
                if (parameter.HasDefaultValue)
                {
                    args.Add(parameter.DefaultValue);
                }
                else
                {
                    throw new InvocationException($"No value found for argument: {argumentName}");
                }
            }
            else
            {
                args.Add(CheckParameter(parameter, argument));
            }

        }

        if (arguments.Count > 0)
        {
            throw new InvocationException("Too many arguments passed");
        }
        
        return DataValue.From(_info.Invoke(_target, args.ToArray()));
    }
}