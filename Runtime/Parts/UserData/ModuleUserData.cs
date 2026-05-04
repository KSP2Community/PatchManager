using System;
using System.Collections.Generic;
using AssetsExporter.Extensions;
using KSP.IO;
using KSP.Sim.Definitions;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Parts.UserData;

/// <summary>
/// Standalone wrapper for a part module's serialized JSON, exposing each <c>ModuleData</c> entry by name with a
/// typed adapter when one is registered for the data type.
/// </summary>
[MoonSharpUserData]
public class ModuleUserData
{
    private JObject _jObject;
    private List<DynValue> _dataValues = new();
    private Dictionary<string, int> _dataIndices = new();

    /// <summary>
    /// Creates the wrapper around the module's serialized JSON.
    /// </summary>
    /// <param name="token">The module's serialized JSON.</param>
    public ModuleUserData(JToken token)
    {
        _jObject = (JObject)token;
        RefreshData();
    }

    /// <summary>
    /// Refreshes the data-name and conversion caches from the current state of the module's <c>ModuleData</c> array.
    /// </summary>
    /// <remarks>
    /// Call after a Lua script clears or rewrites <c>ModuleData</c> directly. Most mutators on this type already refresh.
    /// </remarks>
    public void RefreshData()
    {
        _dataValues.Clear();
        _dataIndices.Clear();
        var data = (JArray)_jObject["ModuleData"];
        var index = 0;
        foreach (var moduleData in data)
        {
            _dataIndices[moduleData["Name"].Value<string>()] = index++;
            _dataValues.Add(GetUserData((JObject)moduleData));
        }
    }

    private DynValue GetUserData(JObject moduleData)
    {
        var type = Type.GetType(moduleData["DataType"].Value<string>());
        if (type != null && PartsUtilities.ModuleDataAdapters.TryGetValue(type, out var adapterType))
        {
            return MoonSharp.Interpreter.UserData.Create(Activator.CreateInstance(type, moduleData));
        }
        return JsonUserData.GetFromJToken(moduleData);
    }

    /// <summary>
    /// Gets or sets the module-data entry with the given name.
    /// </summary>
    /// <param name="idx">The data entry's name.</param>
    /// <returns>The data entry's wrapper, or <see cref="DynValue.Nil" /> if absent.</returns>
    /// <exception cref="Exception">Thrown when setting a name that does not exist; use <see cref="AddData" /> to insert.</exception>
    public DynValue this[string idx]
    {
        get
        {
            if (_dataIndices.TryGetValue(idx, out var index))
            {
                return _dataValues[index];
            }

            return DynValue.Nil;
        }
        set
        {
            if (!_dataIndices.TryGetValue(idx, out var index))
            {
                throw new Exception($"Module Data not found in module {idx}!");
            }

            _jObject["ModuleData"][index] = JsonUserData.GetJTokenForDynValue(value);
            RefreshData();
        }
    }

    /// <summary>
    /// Gets or sets the module-data entry at the given 1-indexed position.
    /// </summary>
    /// <param name="idx">The 1-indexed position.</param>
    /// <returns>The data entry's wrapper, or <see cref="DynValue.Nil" /> if out of range.</returns>
    /// <exception cref="Exception">Thrown when setting an out-of-range position.</exception>
    public DynValue this[int idx]
    {
        get
        {
            if (idx > 0 && idx <= _dataValues.Count)
            {
                return _dataValues[idx-1];
            }

            return DynValue.Nil;
        }
        set
        {
            if (idx > 0 && idx <= _dataValues.Count)
            {
                _jObject["ModuleData"][idx-1] = JsonUserData.GetJTokenForDynValue(value);
                RefreshData();
            }
            else
            {
                throw new Exception("Index out of range for module data!");
            }
        }
    }

    /// <summary>
    /// Returns an iterator yielding each <c>(name, value)</c> pair in declaration order.
    /// </summary>
    /// <returns>The iterator callback.</returns>
    [MoonSharpUserDataMetamethod("__pairs")]
    public DynValue Pairs()
    {
        var iterator = _dataIndices.GetEnumerator();
        return DynValue.NewCallback((sec, args) =>
        {
            if (iterator.MoveNext())
            {
                return DynValue.NewTuple(DynValue.NewString(iterator.Current.Key),_dataValues[iterator.Current.Value]);
            }
            return DynValue.Nil;
        });
    }

    /// <summary>
    /// Gets the number of data entries on this module.
    /// </summary>
    public int Count => _dataValues.Count;

    /// <summary>
    /// Adds a new module-data entry of the given type and runs <paramref name="callback" /> against it.
    /// </summary>
    /// <param name="type">The data module's short name as registered in <c>PartsUtilities.DataModules</c>.</param>
    /// <param name="callback">Callback that receives the new entry for further configuration.</param>
    /// <exception cref="Exception">Thrown when <paramref name="type" /> is not a registered data module.</exception>
    public void AddData(string type, Action<DynValue> callback)
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
        (_jObject["ModuleData"] as JArray)?.Add(trueType);
        var userData = GetUserData(trueType);
        _dataIndices[type] = _dataValues.Count;
        _dataValues.Add(userData);
        callback(userData);
    }

    /// <summary>
    /// Runs <paramref name="callback" /> against the existing data entry of the given type, doing nothing if absent.
    /// </summary>
    /// <param name="type">The data module's short name.</param>
    /// <param name="callback">Callback that receives the existing entry for further configuration.</param>
    public void PatchData(string type, Action<DynValue> callback)
    {
        if (_dataIndices.TryGetValue(type, out var index))
        {
            callback(_dataValues[index]);
        }
    }

    /// <summary>
    /// Patches the named data entry if it exists, otherwise adds it.
    /// </summary>
    /// <param name="type">The data module's short name.</param>
    /// <param name="callback">Callback that receives the entry for further configuration.</param>
    public void EnsureData(string type, Action<DynValue> callback)
    {
        if (_dataIndices.ContainsKey(type))
        {
            PatchData(type, callback);
        }
        else
        {
            AddData(type, callback);
        }
    }

    /// <summary>
    /// Removes the data entry of the given type from the module.
    /// </summary>
    /// <param name="type">The data module's short name.</param>
    public void RemoveData(string type)
    {
        if (!_dataIndices.TryGetValue(type, out var index)) return;
        ((JArray)_jObject["ModuleData"]).RemoveAt(index);
        RefreshData();
    }

    /// <summary>
    /// Returns whether the module has a data entry of the given type.
    /// </summary>
    /// <param name="type">The data module's short name.</param>
    /// <returns>True if an entry exists, false otherwise.</returns>
    public bool HasData(string type)
    {
        return _dataIndices.ContainsKey(type);
    }
}
