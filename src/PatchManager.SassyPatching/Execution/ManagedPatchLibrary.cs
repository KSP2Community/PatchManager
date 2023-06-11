using System.Reflection;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.SassyPatching.Execution;

internal class ManagedPatchLibrary : PatchLibrary
{
    private readonly Dictionary<string, ManagedPatchFunction> _functions;

    public ManagedPatchLibrary(IReflect libraryClass)
    {
        _functions = libraryClass.GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(method => method.GetCustomAttributes().OfType<SassyMethodAttribute>().Any())
            .ToDictionary(method => method.GetCustomAttributes().OfType<SassyMethodAttribute>().First().MethodName,
                method => new ManagedPatchFunction(method));
    }
    
    public override void RegisterInto(Environment environment)
    {
        foreach (var function in _functions)
        {
            environment.GlobalEnvironment.AllFunctions[function.Key] = function.Value;
        }
    }
}