using JetBrains.Annotations;
using KSP.IO;
using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.Parts.Builtins;

/// <summary>
/// This library contains utilities for working with parts in Patch Manager
/// </summary>
[SassyLibrary("builtin","parts")]
[PublicAPI]
public class PartsBuiltins
{
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="partObject">The part data object</param>
    /// <param name="moduleName">The module name</param>
    /// <returns></returns>
    [SassyMethod("find-module")]
    public DataValue FindModule([SassyName("part-object")] Dictionary<string, DataValue> partObject, [SassyName("module-name")] string moduleName)
    {
        if (moduleName.StartsWith("PartComponent"))
            moduleName = moduleName.Replace("PartComponent", "");
        var modules = partObject["serializedPartModules"].List.Select(x => x.Dictionary);
        foreach (var module in modules)
        {
            if (module["Name"].String.Replace("PartComponent", "") == moduleName)
            {
                return module;
            }
        }

        return DataValue.Null;
    }

    [SassyMethod("add-module")]
    public Dictionary<string,DataValue> AddModule([SassyName("part-object")] Dictionary<string, DataValue> partObject, Dictionary<string, DataValue> module)
    {
        var newObject = new Dictionary<string, DataValue>(partObject);
        newObject["serializedPartModules"].List.Add(module);
        return newObject;
    }

    [SassyMethod("remove-module")]
    public Dictionary<string, DataValue> RemoveModule([SassyName("part-object")] Dictionary<string, DataValue> partObject, [SassyName("module-name")] string moduleName)
    {
        if (moduleName.StartsWith("PartComponent"))
            moduleName = moduleName.Replace("PartComponent", "");
        var newObject = new Dictionary<string, DataValue>(partObject);
        var index = -1;
        var modules = partObject["serializedPartModules"].List.Select(x => x.Dictionary).ToList();
        for (var i = 0; i < modules.Count; i++)
        {
            if (modules[i]["Name"].String.Replace("PartComponent", "") != moduleName)
            {
                continue;
            }

            index = i;
            break;
        }

        if (index >= 0)
        {
            newObject["serializedPartModules"].List.RemoveAt(index);
        }

        return newObject;
    }

    [SassyMethod("replace-module")]
    public Dictionary<string, DataValue> ReplaceModule(
        [SassyName("part-object")] Dictionary<string, DataValue> partObject,
        [SassyName("module-name")] string moduleName,
        Dictionary<string, DataValue> module
    )
    {
        if (moduleName.StartsWith("PartComponent"))
            moduleName = moduleName.Replace("PartComponent", "");
        var newObject = new Dictionary<string, DataValue>(partObject);
        var index = -1;
        var modules = partObject["serializedPartModules"].List.Select(x => x.Dictionary).ToList();
        for (var i = 0; i < modules.Count; i++)
        {
            if (modules[i]["Name"].String.Replace("PartComponent", "") != moduleName)
            {
                continue;
            }

            index = i;
            break;
        }

        if (index >= 0)
        {
            newObject["serializedPartModules"].List[index] = module;
        }
        else
        {
            newObject["serializedPartModules"].List.Add(module);
        }
        return newObject;
    }

    [SassyMethod("create-module")]
    public Dictionary<string, DataValue> CreateModule(string type)
    {
        
        if (!PartsUtilities.ComponentModules.TryGetValue(type, out var mod))
        {
            throw new Exception($"Unknown part module {type}");
        }
        var moduleObject = new JObject()
        {
            ["Name"] = mod.componentModule.Name,
            ["ComponentType"] = mod.componentModule.AssemblyQualifiedName,
            ["BehaviourType"] =  mod.behaviour.AssemblyQualifiedName,
            ["ModuleData"] = new JArray()
        };
        return DataValue.FromJToken(moduleObject).Dictionary;
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="partModule"></param>
    /// <param name="moduleDataName"></param>
    /// <returns></returns>
    [SassyMethod("find-module-data")]
    public DataValue FindModuleData([SassyName("part-module")] Dictionary<string, DataValue> partModule, [SassyName("module-data-name")] string moduleDataName)
    {
        var data = partModule["ModuleData"].List.Select(x => x.Dictionary);
        foreach (var moduleData in data)
        {
            if (moduleData["Name"].String == moduleDataName)
            {
                return moduleData;
            }
        }
        return DataValue.Null;
    }
    
    [SassyMethod("add-module-data")]
    public Dictionary<string, DataValue> AddModuleData([SassyName("part-module")] Dictionary<string, DataValue> partModule, [SassyName("module-data")] Dictionary<string, DataValue> moduleData)
    {
        var newObject = new Dictionary<string, DataValue>(partModule);
        newObject["ModuleData"].List.Add(moduleData);
        return newObject;
    }

    [SassyMethod("remove-module-data")]
    public Dictionary<string, DataValue> RemoveModuleData(
        [SassyName("part-module")] Dictionary<string, DataValue> partModule,
        [SassyName("module-data-name")] string moduleDataName
    )
    {
        var newObject = new Dictionary<string, DataValue>(partModule);
        var index = -1;
        var modules = partModule["ModuleData"].List.Select(x => x.Dictionary).ToList();
        for (var i = 0; i < modules.Count; i++)
        {
            if (modules[i]["Name"].String != moduleDataName)
            {
                continue;
            }

            index = i;
            break;
        }

        if (index >= 0)
        {
            newObject["ModuleData"].List.RemoveAt(index);
        }

        return newObject;
    }
    
    [SassyMethod("replace-module-data")]
    public Dictionary<string, DataValue> ReplaceModuleData(
        [SassyName("part-module")] Dictionary<string, DataValue> partModule,
        [SassyName("module-data-name")] string moduleDataName,
        [SassyName("module-data")] Dictionary<string, DataValue> moduleData
    )
    {
        var newObject = new Dictionary<string, DataValue>(partModule);
        var index = -1;
        var modules = partModule["ModuleData"].List.Select(x => x.Dictionary).ToList();
        for (var i = 0; i < modules.Count; i++)
        {
            if (modules[i]["Name"].String != moduleDataName)
            {
                continue;
            }

            index = i;
            break;
        }

        if (index >= 0)
        {
            newObject["ModuleData"].List[index] = moduleData;
        }
        else
        {
            newObject["ModuleData"].List.Add(moduleData);
        }

        return newObject;
    }

    [SassyMethod("create-module-data")]
    public Dictionary<string, DataValue> CreateModuleData(string type)
    {
        if (!PartsUtilities.DataModules.TryGetValue(type, out var dataModuleType))
        {
            throw new Exception($"Unknown data module {type}");
        }
        
        var instance = (ModuleData)Activator.CreateInstance(dataModuleType);
        var dataObject = new JObject
        {
            ["$type"] = $"{dataModuleType.FullName}, {dataModuleType.Assembly.GetName().Name}"
        };
        var otherObject = JObject.Parse(IOProvider.ToJson(instance));
        foreach (var prop in otherObject)
        {
            dataObject[prop.Key] = prop.Value;
        }
        var trueType = new JObject
        {
            ["Name"] =  dataModuleType.Name,
            ["ModuleType"] = instance.ModuleType.AssemblyQualifiedName,
            ["DataType"] = instance.DataType.AssemblyQualifiedName,
            ["Data"] = null,
            ["DataObject"] = dataObject
        };
        return DataValue.FromJToken(trueType).Dictionary;
    }

    [SassyMethod("get-data-object")]
    public static Dictionary<string, DataValue> GetDataObject(
        [SassyName("module-data")] Dictionary<string, DataValue> moduleData
    ) => moduleData["DataObject"].Dictionary;

    [SassyMethod("set-data-object")]
    public static Dictionary<string, DataValue> SetDataObject(
        [SassyName("module-data")] Dictionary<string, DataValue> moduleData,
        [SassyName("data-object")] Dictionary<string, DataValue> dataObject
    )
    {
        var result = new Dictionary<string, DataValue>(moduleData);
        result["DataObject"] = dataObject;
        return result;
    }
}