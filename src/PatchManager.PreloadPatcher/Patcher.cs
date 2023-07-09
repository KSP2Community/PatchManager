using BepInEx;
using BepInEx.Logging;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace PatchManager.PreloadPatcher;

/// <summary>
/// Preload patcher for the game's AssetProvider.
/// </summary>
public static class Patcher
{
    [UsedImplicitly]
    public static IEnumerable<string> TargetDLLs { get; } = new[]
    {
        "Assembly-CSharp.dll"
    };

    private static MethodReference MakeGeneric(this MethodReference method, params GenericParameter[] args)
    {
        if (args.Length == 0)
        {
            return method;
        }

        if (method.GenericParameters.Count != args.Length)
        {
            throw new ArgumentException("Invalid number of generic type arguments supplied");
        }

        var genericTypeRef = new GenericInstanceMethod(method);
        foreach (var arg in args)
        {
            genericTypeRef.GenericArguments.Add(arg);
        }

        return genericTypeRef;
    }

    /// <summary>
    /// Replace the generic AssetProvider.LoadAssetsAsync method with our own.
    /// </summary>
    /// <param name="assemblyDefinition">Game assembly containing the AssetProvider class.</param>
    /// <exception cref="Exception">Thrown if the assembly with the replacement method cannot be found.</exception>
    [UsedImplicitly]
    public static void Patch(ref AssemblyDefinition assemblyDefinition)
    {
        // Now we need to get the assembly with the replacement method
        AssemblyDefinition coreAssembly = null;
        var dir = new DirectoryInfo(Paths.PluginPath);
        foreach (var file in dir.EnumerateFiles("PatchManager.Core.dll", SearchOption.AllDirectories))
        {
            coreAssembly = AssemblyDefinition.ReadAssembly(file.FullName);
        }

        if (coreAssembly == null)
        {
            throw new Exception("Could not find PatchManager Core");
        }

        var coreType = coreAssembly.MainModule.Types.First(t => t.Name == "AssetProviderPatch");
        var extractedMethod = coreType.Methods.First(m => m.Name == "LoadAssetsAsync");

        var targetType = assemblyDefinition.MainModule.Types.Single(t => t.Name == "AssetProvider");
        var targetMethod = targetType.Methods.Single(m => m.Name == "LoadAssetsAsync" && m.HasGenericParameters);
        // Remove every single instruction from the body of the methods
        // Emit call to our extracted method
        var methodInModule = targetMethod.Module.ImportReference(extractedMethod);
        var generic = methodInModule.MakeGeneric(targetMethod.GenericParameters.ToArray());
        targetMethod.Body.Instructions.Clear();
        targetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        targetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
        targetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, generic));
        targetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

        Logger.CreateLogSource("Patch Manager Preload").LogInfo("Pre-patching complete!");
    }
}