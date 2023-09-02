using KSP.Sim.Definitions;
using KSP.Sim.impl;
using PatchManager.Parts.Attributes;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.Parts;

/// <summary>
/// Utilities for parts patching.
/// </summary>
public static class PartsUtilities
{
    private static Dictionary<string, (Type componentModule, Type behaviour)> _componentModules;

    private static void BuildComponentModuleDictionary()
    {
        _componentModules = new();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes().Where(type => !type.IsAbstract).Where(type => type.IsSubclassOf(typeof(PartComponentModule))))
            {
                try
                {
                    var mod = (PartComponentModule)Activator.CreateInstance(type);
                    var behaviour = mod.PartBehaviourModuleType;
                    var tuple = (type, behaviour);
                    var name1 = type.Name;
                    var name2 = name1.Replace("PartComponent", "");
                    _componentModules[name1] = tuple;
                    if (!name1.Equals(name2))
                    {
                        _componentModules[name2] = tuple;
                    }
                }
                catch
                {
                    //ignored
                }
            }
        }
    }

    internal static IReadOnlyDictionary<string, (Type componentModule, Type behaviour)> ComponentModules
    {
        get
        {
            if (_componentModules == null)
            {
                BuildComponentModuleDictionary();
            }

            return _componentModules;
        }
    }


    private static Dictionary<string, Type> _dataModules;

    private static void BuildDataModuleDictionary()
    {
        _dataModules = new();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes().Where(type => !type.IsAbstract)
                         .Where(type => type.IsSubclassOf(typeof(ModuleData))))
            {
                var name1 = type.Name;
                var name2 = name1.Replace("Data_", "");
                _dataModules[name1] = type;
                if (!name1.Equals(name2))
                {
                    _dataModules[name2] = type;
                }
            }
        }
    }

    internal static IReadOnlyDictionary<string, Type> DataModules
    {
        get
        {
            if (_dataModules == null)
            {
                BuildDataModuleDictionary();
            }

            return _dataModules;
        }
    }

    internal static readonly Dictionary<Type, Type> ModuleDataAdapters = new();

    internal static void GrabModuleDataAdapters()
    {
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                     .SelectMany(x => x.GetTypes())
                     .Where(x => x.GetCustomAttributes(typeof(ModuleDataAdapterAttribute),
                             false)
                         .Any())
                     .Select(x => (type: x, attr: (ModuleDataAdapterAttribute)x.GetCustomAttributes(typeof(ModuleDataAdapterAttribute),
                             false)
                         .FirstOrDefault())))
        {
            foreach (var dataType in type.attr.ValidTypes)
            {
                ModuleDataAdapters[dataType] = type.type;
            }
        }
    }
    public static void RegisterModuleDataAdapter<T>(params Type[] validTargets) where T : ISelectable
    {
        foreach (var type in validTargets)
        {
            ModuleDataAdapters[type] = typeof(T);
        }
    }
}