using System.Reflection;
using MonoMod.Utils;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.SassyPatching.Execution;

internal class ManagedPatchLibrary : PatchLibrary
{
    private readonly Dictionary<string, ManagedPatchFunction> _functions;
    private readonly Dictionary<string, DataValue> _constants;

    public ManagedPatchLibrary(IReflect libraryClass)
    {
        _functions = libraryClass.GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(method => method.GetCustomAttributes().OfType<SassyMethodAttribute>().Any())
            .ToDictionary(method => method.GetCustomAttributes().OfType<SassyMethodAttribute>().First().MethodName,
                method => new ManagedPatchFunction(method));
        _constants = libraryClass.GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(field => field.GetCustomAttributes().OfType<SassyConstantAttribute>().Any()).ToDictionary(
                field => field.GetCustomAttributes().OfType<SassyConstantAttribute>().First().ConstantName,
                field => DataValue.From(field.GetValue(null)));
        _constants.AddRange(libraryClass.GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(property => property.GetCustomAttributes().OfType<SassyConstantAttribute>().Any()).ToDictionary(
                property => property.GetCustomAttributes().OfType<SassyConstantAttribute>().First().ConstantName,
                property => DataValue.From(property.GetValue(null))));
    }
    
    public override void RegisterInto(Environment environment)
    {
        foreach (var function in _functions)
        {
            environment.GlobalEnvironment.Universe.MessageLogger($"Registering function: {function.Key}");
            environment.GlobalEnvironment.AllFunctions[function.Key] = function.Value;
        }

        foreach (var constant in _constants)
        {
            environment.GlobalEnvironment.Universe.MessageLogger($"Registering constant: {constant.Key}");
            environment[constant.Key] = constant.Value;
        }
    }
}