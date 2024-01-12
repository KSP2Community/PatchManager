using BepInEx;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpaceWarp.Preload.API;

namespace PatchManager.PreloadPatcher;

/// <summary>
/// Preload patcher for the game's AssetProvider.
/// </summary>
[UsedImplicitly]
internal class Patcher : BasePatcher
{
    public override IEnumerable<string> DLLsToPatch { get; } = new[]
    {
        "Assembly-CSharp.dll"
    };

    private static MethodReference MakeGeneric(MethodReference method, params GenericParameter[] args)
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
    public override void ApplyPatch(ref AssemblyDefinition assemblyDefinition)
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
        var extractedMethod = coreType.Methods.First(m => m.Name == "LoadByLabel");

        var targetType = assemblyDefinition.MainModule.Types.Single(t => t.Name == "AssetProvider");
        var targetMethod = targetType.Methods.Single(m => m.Name == "LoadByLabel" && m.HasGenericParameters);
        // Remove every single instruction from the body of the methods
        // Emit call to our extracted method
        var methodInModule = targetMethod.Module.ImportReference(extractedMethod);
        var generic = MakeGeneric(methodInModule, targetMethod.GenericParameters.ToArray());
        targetMethod.Body.Instructions.Clear();
        targetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        targetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
        targetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_3));
        targetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, generic));
        targetMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

        Logger.LogInfo("Pre-patching complete!");
    }
}