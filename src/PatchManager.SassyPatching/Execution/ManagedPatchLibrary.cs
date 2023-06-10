using System.Reflection;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.SassyPatching.Execution;

internal class ManagedPatchLibrary : PatchLibrary
{
    public Dictionary<string, ManagedPatchFunction> Functions;

    public ManagedPatchLibrary(IReflect libraryClass)
    {
        Functions = libraryClass.GetMethods(BindingFlags.Static)
            .Where(method => method.GetCustomAttributes().OfType<SassyMethodAttribute>().Any())
            .ToDictionary(method => method.GetCustomAttributes().OfType<SassyMethodAttribute>().First().MethodName,
                method => new ManagedPatchFunction(method));
    }
    
    public override void RegisterInto(Environment environment)
    {
        foreach (var function in Functions)
        {
            environment.GlobalEnvironment.AllFunctions[function.Key] = function.Value;
        }
    }
}