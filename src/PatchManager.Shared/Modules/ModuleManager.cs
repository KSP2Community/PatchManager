using System.Reflection;
using JetBrains.Annotations;

namespace PatchManager.Shared.Modules;

/// <summary>
/// Manages the loading of PatchManager DLL modules.
/// </summary>
internal static class ModuleManager
{
    private static readonly HashSet<string> ModulePaths = new();
    internal static readonly List<IModule> Modules = new();

    /// <summary>
    /// Registers a PatchManager module DLL to be loaded. The module must contain a single class that inherits
    /// from <see cref="IModule"/>.
    /// </summary>
    /// <param name="path">Path to the module DLL file</param>
    [PublicAPI]
    public static void Register(string path)
    {
        if (ModulePaths.Contains(path))
        {
            Logging.LogError($"Module {path} is already registered.");
            return;
        }

        var assembly = Assembly.LoadFile(path);
        var modules = assembly.ExportedTypes
            .Where(type => typeof(IModule).IsAssignableFrom(type))
            .ToList();

        if (modules.Count != 1)
        {
            Logging.LogError(
                $"Assembly {assembly.FullName} should contain a single class implementing {typeof(IModule).FullName}"
            );
            return;
        }

        var instance = (IModule)Activator.CreateInstance(modules[0]);
        Modules.Add(instance);
        ModulePaths.Add(path);
    }

    internal static void InitAll()
    {
        foreach (var module in Modules)
        {
            module.Init();
        }
    }

    internal static void LoadAll()
    {
        foreach (var module in Modules)
        {
            module.Load();
        }
    }

    internal static void PreLoadAll()
    {
        
        foreach (var module in Modules)
        {
            module.PreLoad();
        }
    }
}