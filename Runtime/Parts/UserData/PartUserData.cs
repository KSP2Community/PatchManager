using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;
using UnityEditor.Experimental.GraphView;

namespace PatchManager.Parts.UserData;


[MoonSharpUserData]
public class PartUserData : ExtensibleJsonUserData
{
    public JToken FullToken;
    private readonly Dictionary<string, int> _moduleIndices = new();
    private readonly Dictionary<string, DynValue> _partModules = new();
    private readonly DynValue _resourceUserData;

    public PartUserData(JToken token) : base(token["data"])
    {
        FullToken = token;
        _resourceUserData =
            MoonSharp.Interpreter.UserData.Create(new ResourceContainersUserData((JArray)token["data"]["resourceContainers"]));
        var index = 0;
        foreach (var module in Token["serializedPartModules"])
        {
            _moduleIndices[module["Name"].Value<string>().Replace("PartComponent", "")] = index++;
            _partModules[module["Name"].Value<string>().Replace("PartComponent", "")] = MoonSharp.Interpreter.UserData.Create(new ModuleUserData(module));
        }
    }

    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "serializedPartModules";
        yield return "resourceContainers";
        foreach (var module in _moduleIndices.Keys)
        {
            yield return module;
            yield return "PartComponent" + module;
        }
    }

    public override DynValue TryToGet(string property)
    {
        if (property == "serializedPartModules")
        {
            // Note, users should not edit this directly, but if they want to they can
            return GetFromJToken(Token["data"]!["serializedPartModules"]);
        }
        if (property == "resourceContainers") return _resourceUserData;
        if (_partModules.TryGetValue(property, out var module)) return module;
        if (_partModules.TryGetValue(property.Replace("PartComponent",""), out module)) return module;
        return null;
    }

    public override bool TryToSet(string property, DynValue value)
    {
        if (property == "serializedPartModules")
        {
            throw new Exception("Use the part methods to access part modules!");
        }

        if (property == "resourceContainers")
        {
            throw new Exception("Use the resource methods to access resource containers!");
        }

        if (_partModules.ContainsKey(property) || _partModules.ContainsKey(property.Replace("PartComponent","")))
        {
            throw new Exception("Use the module patching methods to update part modules!");
        }

        return false;
    }

    public override bool TryToRemove(string property)
    {
        throw new Exception("Use the relevant methods to add or remove properties/modules!");
    }

    public void AddModule(string moduleType, Action<ModuleUserData> callback)
    {
        if (!PartsUtilities.ComponentModules.TryGetValue(moduleType, out var mod))
        {
            throw new Exception($"Unknown part module {moduleType}");
        }

        var moduleObject = new JObject()
        {
            ["Name"] = mod.componentModule.Name,
            ["ComponentType"] = mod.componentModule.AssemblyQualifiedName,
            ["BehaviourType"] =  mod.behaviour.AssemblyQualifiedName,
            ["ModuleData"] = new JArray()
        };
        (Token["data"]["serializedPartModules"] as JArray)?.Add(moduleObject);
        _moduleIndices[moduleType.Replace("PartComponent", "")] =
            (Token["data"]["serializedPartModules"] as JArray)!.Count;

        var typed = new ModuleUserData(moduleObject);
        var ud = MoonSharp.Interpreter.UserData.Create(typed);
        _partModules.Add(moduleType.Replace("PartComponent", ""), ud);
        callback(typed);
    }

    public void PatchModule(string moduleType, Action<ModuleUserData> callback)
    {
        if (_partModules.TryGetValue(moduleType.Replace("PartComponent", ""), out var mod))
        {
            callback((ModuleUserData)mod.UserData.Object);
        }
    }

    public void EnsureModule(string moduleType, Action<ModuleUserData> callback)
    {
        if (_partModules.TryGetValue(moduleType.Replace("PartComponent", ""), out var mod))
        {
            callback((ModuleUserData)mod.UserData.Object);
        }
        else
        {
            AddModule(moduleType, callback);
        }
    }

    public void RemoveModule(string moduleType)
    {
        // _moduleIndices.Remove(moduleType.Replace("PartComponent", ""));
        _partModules.Remove(moduleType.Replace("PartComponent", ""));
        _moduleIndices.Clear();
        var index = 0;
        foreach (var module in Token["serializedPartModules"])
        {
            _moduleIndices[module["Name"].Value<string>().Replace("PartComponent", "")] = index++;
        }
    }
    
    public bool HasModule(string moduleType) => _moduleIndices.ContainsKey(moduleType.Replace("PartComponent", ""));

    #region Part Utility Methods
    

    #endregion
}