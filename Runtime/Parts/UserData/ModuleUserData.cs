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

[MoonSharpUserData]
public class ModuleUserData
{
    private JObject _jObject;
    private List<DynValue> _dataValues = new();
    private Dictionary<string, int> _dataIndices = new();
    public ModuleUserData(JToken token)
    {
        _jObject = (JObject)token;
        RefreshData();
    }

    /// <summary>
    /// Refreshes the data modules (can be used when someone runs a Clear on the data like a silly goose)
    /// </summary>
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
    
    public int Count => _dataValues.Count;

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

    public void PatchData(string type, Action<DynValue> callback)
    {
        if (_dataIndices.TryGetValue(type, out var index))
        {
            callback(_dataValues[index]);
        }
    }

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

    public void RemoveData(string type)
    {
        RefreshData();
    }

    public bool HasData(string type)
    {
        return _dataIndices.ContainsKey(type);
    }
}