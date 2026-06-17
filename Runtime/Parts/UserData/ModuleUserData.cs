using System;
using JetBrains.Annotations;
using KSP.IO;
using KSP.Sim.Definitions;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Parts.UserData;

/// <summary>
/// Wrapper for a part module's serialized JSON, exposing each <c>ModuleData</c> entry by name (and by 1-based
/// position from Lua) with a typed adapter when one is registered for the data type. Backed by the module's
/// <c>ModuleData</c> array as a name-indexed list.
/// </summary>
[MoonSharpUserData]
public class ModuleUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper over the module's <c>ModuleData</c> array.
    /// </summary>
    /// <param name="token">The module's serialized JSON object.</param>
    public ModuleUserData(JToken token) : base(RequireArray(token["ModuleData"], "module ModuleData"))
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source) => RequireString(source["Name"], "ModuleData[].Name");

    /// <inheritdoc />
    [MoonSharpHidden]
    public override DynValue Convert(JToken source)
    {
        var moduleData = RequireObject(source, "ModuleData entry");
        var dataTypeName = RequireString(moduleData["DataType"], "ModuleData entry's DataType");
        var type = Type.GetType(dataTypeName);
        var dataObject = RequireObject(moduleData["DataObject"], "ModuleData entry's DataObject");
        if (type != null && PartsUtilities.ModuleDataAdapters.TryGetValue(type, out var adapterType))
        {
            try
            {
                return MoonSharp.Interpreter.UserData.Create(Activator.CreateInstance(adapterType, dataObject));
            }
            catch (Exception e) when (e is not ScriptRuntimeException)
            {
                var inner = (e as System.Reflection.TargetInvocationException)?.InnerException ?? e;
                throw new ScriptRuntimeException($"Failed to construct module-data adapter '{adapterType.FullName}' for type '{dataTypeName}': {inner.Message}");
            }
        }
        return GetFromJToken(dataObject);
    }

    /// <inheritdoc />
    [MoonSharpHidden]
    protected override DynValue TryGetVirtual(DynValue key)
    {
        // Numeric access returns the typed conversion (the adapter), matching name access. The base list would
        // otherwise hand back the raw entry JSON for a numeric index.
        if (key.Type == DataType.Number)
        {
            var i = (int)key.Number - 1;
            return i >= 0 && i < Conversions.Count ? Conversions[i] : DynValue.Nil;
        }
        return base.TryGetVirtual(key);
    }

    /// <inheritdoc />
    [MoonSharpHidden]
    protected override bool TrySetVirtual(DynValue key, DynValue value)
    {
        // Unlike a plain indexed list, a module data entry can be replaced in place by name or position.
        int index;
        if (key.Type == DataType.String)
        {
            if (!Indices.TryGetValue(key.String, out index))
                throw new ScriptRuntimeException($"Module Data not found in module {key.String}!");
        }
        else if (key.Type == DataType.Number)
        {
            index = (int)key.Number - 1;
            if (index < 0 || index >= List.Count)
                throw new ScriptRuntimeException("Index out of range for module data!");
        }
        else
        {
            return false;
        }

        List[index] = GetJTokenForDynValue(value);
        HardRefresh();
        return true;
    }

    /// <summary>
    /// Adds a new module-data entry of the given type and runs <paramref name="callback" /> against it when supplied.
    /// </summary>
    /// <param name="type">The data module's short name as registered in <c>PartsUtilities.DataModules</c>.</param>
    /// <param name="callback">Optional callback that receives the new entry for further configuration.</param>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="type" /> is not a registered data module.</exception>
    public void AddData(string type, [CanBeNull] Action<DynValue> callback = null)
    {
        if (!PartsUtilities.DataModules.TryGetValue(type, out var dataModuleType))
        {
            throw new ScriptRuntimeException($"Unknown data module {type}");
        }

        ModuleData instance;
        try
        {
            instance = (ModuleData)Activator.CreateInstance(dataModuleType);
        }
        catch (Exception e) when (e is not ScriptRuntimeException)
        {
            throw new ScriptRuntimeException($"Failed to construct data module '{type}' ({dataModuleType.FullName}): {e.Message}");
        }

        var dataObject = new JObject
        {
            ["$type"] = $"{dataModuleType.FullName}, {dataModuleType.Assembly.GetName().Name}"
        };
        JObject otherObject;
        try
        {
            otherObject = JObject.Parse(IOProvider.ToJson(instance));
        }
        catch (Exception e) when (e is not ScriptRuntimeException)
        {
            throw new ScriptRuntimeException($"Failed to serialize default data for module '{type}' ({dataModuleType.FullName}): {e.Message}");
        }
        foreach (var prop in otherObject)
        {
            dataObject[prop.Key] = prop.Value;
        }
        var trueType = new JObject
        {
            ["Name"] = dataModuleType.Name,
            ["ModuleType"] = instance.ModuleType.AssemblyQualifiedName,
            ["DataType"] = instance.DataType.AssemblyQualifiedName,
            ["Data"] = null,
            ["DataObject"] = dataObject
        };
        Append(GetFromJToken(trueType));
        callback?.Invoke(Conversions[Conversions.Count - 1]);
    }

    /// <summary>
    /// Runs <paramref name="callback" /> against the existing data entry of the given type, doing nothing if absent.
    /// </summary>
    /// <param name="type">The data module's short name.</param>
    /// <param name="callback">Callback that receives the existing entry for further configuration.</param>
    public void PatchData(string type, Action<DynValue> callback)
    {
        if (Indices.TryGetValue(type, out var index))
        {
            callback(Conversions[index]);
        }
    }

    /// <summary>
    /// Patches the named data entry if it exists, otherwise adds it.
    /// </summary>
    /// <param name="type">The data module's short name.</param>
    /// <param name="callback">Callback that receives the entry for further configuration.</param>
    public void EnsureData(string type, Action<DynValue> callback)
    {
        if (Indices.ContainsKey(type))
        {
            PatchData(type, callback);
        }
        else
        {
            AddData(type, callback);
        }
    }

    /// <summary>
    /// Ensures a data entry of the given type exists, then runs <paramref name="callback" /> against its raw DataObject JSON.
    /// </summary>
    /// <param name="type">The data module's short name.</param>
    /// <param name="callback">Callback that receives the entry's DataObject for further configuration.</param>
    [MoonSharpHidden]
    public void EnsureDataObject(string type, Action<JObject> callback)
    {
        EnsureData(type, _ => callback(GetDataObject(type)));
    }

    /// <summary>
    /// Returns the raw DataObject JSON for the data entry of the given type.
    /// </summary>
    /// <param name="type">The data module's short name.</param>
    /// <returns>The entry's DataObject.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when no entry of the given type exists.</exception>
    [MoonSharpHidden]
    public JObject GetDataObject(string type)
    {
        if (!Indices.TryGetValue(type, out var index))
        {
            throw new ScriptRuntimeException($"Module Data not found in module {type}!");
        }

        return RequireObject(List[index]["DataObject"], "ModuleData entry's DataObject");
    }

    /// <summary>
    /// Removes the data entry of the given type from the module, doing nothing if absent.
    /// </summary>
    /// <param name="type">The data module's short name.</param>
    public void RemoveData(string type)
    {
        if (Indices.ContainsKey(type)) Remove(type);
    }

    /// <summary>
    /// Returns whether the module has a data entry of the given type.
    /// </summary>
    /// <param name="type">The data module's short name.</param>
    /// <returns>True if an entry exists, false otherwise.</returns>
    public bool HasData(string type) => Indices.ContainsKey(type);

    /// <summary>
    /// Returns the data entry for data type <typeparamref name="TData" /> as a JsonUserData, or null when absent.
    /// The instance is the registered typed adapter when one exists for the data type, otherwise a raw wrapper.
    /// </summary>
    /// <typeparam name="TData">The Data_* class identifying the entry.</typeparam>
    [MoonSharpHidden]
    public JsonUserData GetData<TData>()
    {
        return Indices.TryGetValue(typeof(TData).Name, out var index)
            ? Conversions[index].UserData?.Object as JsonUserData
            : null;
    }

    /// <summary>
    /// Returns the data entry for data type <typeparamref name="TData" /> cast to its adapter type, or null when
    /// absent or not of that type.
    /// </summary>
    /// <typeparam name="TData">The Data_* class identifying the entry.</typeparam>
    /// <typeparam name="TAdapter">The adapter type to cast the entry to.</typeparam>
    [MoonSharpHidden]
    public TAdapter GetData<TData, TAdapter>() where TAdapter : class
    {
        return Indices.TryGetValue(typeof(TData).Name, out var index)
            ? Conversions[index].UserData?.Object as TAdapter
            : null;
    }
}
