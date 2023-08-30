using KSP.Sim.Definitions;
using KSP.Sim.impl;

namespace PatchManager.Parts;

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
                    var name2 = name1.Replace("PartComponentModule_", "");
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
}