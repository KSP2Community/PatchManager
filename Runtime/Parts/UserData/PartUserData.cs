using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;
using UnityEditor.Experimental.GraphView;

namespace PatchManager.Parts.UserData;


/// <summary>
/// Part definition wrapper exposing the inner <c>data</c> subtree, the typed <c>resourceContainers</c> array, and
/// each part module as a virtual property keyed by the module's short name.
/// </summary>
/// <remarks>
/// Module names are normalized by stripping the <c>PartComponent</c> prefix, so both <c>"FuelTank"</c> and
/// <c>"PartComponentFuelTank"</c> resolve to the same module wrapper. Mutating module configuration through the
/// virtual properties is disallowed; use <see cref="AddModule" />, <see cref="PatchModule" />, or
/// <see cref="RemoveModule" /> instead.
/// </remarks>
[MoonSharpUserData]
public class PartUserData : ExtensibleJsonUserData
{
    /// <summary>
    /// The complete JSON envelope, mutated indirectly through the inner <c>data</c> subtree.
    /// </summary>
    public JToken FullToken;

    private readonly Dictionary<string, int> _moduleIndices = new();
    private readonly Dictionary<string, DynValue> _partModules = new();
    private readonly DynValue _resourceUserData;

    /// <summary>
    /// Creates a wrapper for the part envelope, exposing the inner <c>data</c> subtree and seeding the per-module caches.
    /// </summary>
    /// <param name="token">The part JSON envelope (containing a <c>data</c> object).</param>
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override bool TryToRemove(string property)
    {
        throw new Exception("Use the relevant methods to add or remove properties/modules!");
    }

    /// <summary>
    /// Adds a new part module of the given type and runs <paramref name="callback" /> against it for further configuration.
    /// </summary>
    /// <param name="moduleType">The module's short name (with or without the <c>PartComponent</c> prefix).</param>
    /// <param name="callback">Callback that receives the new module for further configuration.</param>
    /// <exception cref="Exception">Thrown when <paramref name="moduleType" /> is not a known component module.</exception>
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

    /// <summary>
    /// Runs <paramref name="callback" /> against the existing module of the given type, doing nothing if absent.
    /// </summary>
    /// <param name="moduleType">The module's short name (with or without the <c>PartComponent</c> prefix).</param>
    /// <param name="callback">Callback that receives the existing module for further configuration.</param>
    public void PatchModule(string moduleType, Action<ModuleUserData> callback)
    {
        if (_partModules.TryGetValue(moduleType.Replace("PartComponent", ""), out var mod))
        {
            callback((ModuleUserData)mod.UserData.Object);
        }
    }

    /// <summary>
    /// Patches the named module if it exists, otherwise adds it.
    /// </summary>
    /// <param name="moduleType">The module's short name (with or without the <c>PartComponent</c> prefix).</param>
    /// <param name="callback">Callback that receives the module for further configuration.</param>
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

    /// <summary>
    /// Removes the named module from the part and rebuilds the module-index map.
    /// </summary>
    /// <param name="moduleType">The module's short name (with or without the <c>PartComponent</c> prefix).</param>
    public void RemoveModule(string moduleType)
    {
        var normalized = moduleType.Replace("PartComponent", "");
        if (!_moduleIndices.TryGetValue(normalized, out var index)) return;
        ((JArray)Token["serializedPartModules"]).RemoveAt(index);
        _partModules.Remove(normalized);
        _moduleIndices.Clear();
        var i = 0;
        foreach (var module in Token["serializedPartModules"])
        {
            _moduleIndices[module["Name"].Value<string>().Replace("PartComponent", "")] = i++;
        }
    }

    /// <summary>
    /// Returns whether the part has a module of the given type.
    /// </summary>
    /// <param name="moduleType">The module's short name (with or without the <c>PartComponent</c> prefix).</param>
    /// <returns>True if a module of that type is registered, false otherwise.</returns>
    public bool HasModule(string moduleType) => _moduleIndices.ContainsKey(moduleType.Replace("PartComponent", ""));

    #region Part Utility Methods


    #endregion
}
