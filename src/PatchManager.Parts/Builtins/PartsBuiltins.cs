using JetBrains.Annotations;
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
    public DataValue FindModule(Dictionary<string, DataValue> partObject, string moduleName)
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
    public Dictionary<string,DataValue> AddModule(Dictionary<string, DataValue> partObject, Dictionary<string, DataValue> module)
    {
        var newObject = new Dictionary<string, DataValue>(partObject);
        newObject["serializedPartModules"].List.Add(module);
        return newObject;
    }

    [SassyMethod("remove-module")]
    public Dictionary<string, DataValue> RemoveModule(Dictionary<string, DataValue> partObject, string moduleName)
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
        Dictionary<string, DataValue> partObject,
        string moduleName,
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
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="partModule"></param>
    /// <param name="moduleDataName"></param>
    /// <returns></returns>
    [SassyMethod("find-module-data")]
    public DataValue FindModuleData(Dictionary<string, DataValue> partModule, string moduleDataName)
    {
        var data = partModule["ModuleData"].List.Select(x => x.Dictionary);
        foreach (var moduleData in data)
        {
            if (moduleData["Name"].String == moduleDataName)
            {
                return moduleData["DataObject"];
            }
        }
        return DataValue.Null;
    }
    
    [SassyMethod("add-module-data")]
    public Dictionary<string, DataValue> AddModuleData(Dictionary<string, DataValue> partModule, Dictionary<string, DataValue> moduleData)
    {
        var newObject = new Dictionary<string, DataValue>(partModule);
        newObject["ModuleData"].List.Add(moduleData);
        return newObject;
    }

    [SassyMethod("remove-module-data")]
    public Dictionary<string, DataValue> RemoveModuleData(
        Dictionary<string, DataValue> partModule,
        string moduleDataName
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
        Dictionary<string, DataValue> partModule,
        string moduleDataName,
        Dictionary<string, DataValue> moduleData
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
    
}