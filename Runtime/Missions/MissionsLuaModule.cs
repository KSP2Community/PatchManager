using System;
using KSP.Game.Missions;
using KSP.Game.Missions.Definitions;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Missions.UserData;

namespace PatchManager.Missions;

/// <summary>
/// Lua submodule exposed as <c>PM.Missions</c>, providing patches and creation helpers for missions, stages,
/// conditions, and actions.
/// </summary>
[PatchManagerModule("Missions")]
[MoonSharpUserData]
public class MissionsLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    /// <summary>
    /// Creates the submodule bound to the given core and universe.
    /// </summary>
    /// <param name="pmc">The shared <c>PM</c> core instance.</param>
    /// <param name="universe">The owning universe.</param>
    public MissionsLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }
    
    /// <summary>
    /// Registers a mission patch with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// The patch matches every mission by default; restrict it via <see cref="LuaPatch.Named" />, which supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    /// <param name="script">The host Lua script; its <c>ModId</c> global is used to namespace <paramref name="name" />.</param>
    /// <param name="name">The patch's local name; namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch Patch(Script script, string name)
    {
        return _core.Patch(script, "Mission", "missions", name);
    }

    #region Utility Methods
    /// <summary>
    /// Returns the assembly-qualified type name of the property watcher registered under <paramref name="name" />.
    /// </summary>
    /// <remarks>
    /// Used in mission JSON to populate <c>$type</c> fields without hardcoding fully-qualified names in Lua.
    /// </remarks>
    /// <param name="name">The watcher's short name as registered in <c>MissionsTypes.PropertyWatchers</c>.</param>
    /// <returns>The assembly-qualified type name of the watcher.</returns>
    public string GetPropertyWatcher(string name)
        => MissionsTypes.PropertyWatchers[name].AssemblyQualifiedName;

    /// <summary>
    /// Returns the assembly-qualified type name of the message registered under <paramref name="name" />.
    /// </summary>
    /// <remarks>
    /// Used in mission JSON to populate <c>$type</c> fields without hardcoding fully-qualified names in Lua.
    /// </remarks>
    /// <param name="name">The message's short name as registered in <c>MissionsTypes.Messages</c>.</param>
    /// <returns>The assembly-qualified type name of the message.</returns>
    public string GetMessage(string name)
        => MissionsTypes.Messages[name].AssemblyQualifiedName;
    #endregion

    #region Creation
    /// <summary>
    /// Creates a new mission stage with the given name and runs <paramref name="callback" /> against it for
    /// further configuration.
    /// </summary>
    /// <param name="name">The stage name.</param>
    /// <param name="callback">Callback that receives the new stage for further configuration.</param>
    /// <returns>The created stage.</returns>
    public StageUserData CreateStage(string name, Action<StageUserData> callback)
    {
        var obj = new MissionStage
        {
            name = name,
        };
        var stage = JObject.FromObject(obj);
        var typed = new StageUserData(stage);
        callback(typed);
        return typed;
    }

    /// <summary>
    /// Returns a condition set that requires every supplied condition to be true.
    /// </summary>
    /// <param name="arguments">The conditions to combine.</param>
    /// <returns>A condition-set wrapper representing the conjunction.</returns>
    public DynValue And(CallbackArguments arguments)
    {
        var conditionSet = new ConditionSet
        {
            ConditionMode = LogicalOperator.AND
        };
        var obj = JObject.FromObject(conditionSet);
        var children = JsonUserData.RequireArray(obj["Children"], "ConditionSet.Children");
        foreach (var arg in arguments.GetArray())
        {
            children.Add(JsonUserData.GetJTokenForDynValue(arg));
        }
        return JsonUserData.GetFromJToken(obj);
    }

    /// <summary>
    /// Returns a condition set that requires at least one supplied condition to be true.
    /// </summary>
    /// <param name="arguments">The conditions to combine.</param>
    /// <returns>A condition-set wrapper representing the disjunction.</returns>
    public DynValue Or(CallbackArguments arguments)
    {
        var conditionSet = new ConditionSet
        {
            ConditionMode = LogicalOperator.OR
        };
        var obj = JObject.FromObject(conditionSet);
        var children = JsonUserData.RequireArray(obj["Children"], "ConditionSet.Children");
        foreach (var arg in arguments.GetArray())
        {
            children.Add(JsonUserData.GetJTokenForDynValue(arg));
        }
        return JsonUserData.GetFromJToken(obj);
    }

    /// <summary>
    /// Returns a condition set that inverts the supplied condition.
    /// </summary>
    /// <param name="condition">The condition to negate.</param>
    /// <returns>A condition-set wrapper representing the negation.</returns>
    public DynValue Not(DynValue condition)
    {
        var conditionSet = new ConditionSet
        {
            ConditionMode = LogicalOperator.NOT
        };
        var obj = JObject.FromObject(conditionSet);
        JsonUserData.RequireArray(obj["Children"], "ConditionSet.Children").Add(JsonUserData.GetJTokenForDynValue(condition));
        return JsonUserData.GetFromJToken(obj);
    }

    /// <summary>
    /// Creates a new mission action of the given short type name and runs <paramref name="callback" /> against
    /// the underlying JSON for further configuration.
    /// </summary>
    /// <param name="type">The action's short name as registered in <c>MissionsTypes.Actions</c>.</param>
    /// <param name="callback">Callback that receives the action's JSON for further configuration.</param>
    /// <returns>A wrapper around the configured action JSON.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="type" /> resolves to a class without a parameterless constructor.</exception>
    public DynValue Action(string type, Action<JsonUserData> callback)
    {
        if (!MissionsTypes.Actions.TryGetValue(type, out var actualType))
        {
            throw new ScriptRuntimeException($"Unknown mission action type '{type}'.");
        }

        object instance;
        try
        {
            instance = Activator.CreateInstance(actualType);
        }
        catch (MissingMethodException e)
        {
            throw new ScriptRuntimeException($"Mission action type {actualType.FullName} (registered as \"{type}\") must declare a parameterless constructor to be instantiable by PM.Missions:Action ({e.Message})");
        }
        catch (Exception e) when (e is not ScriptRuntimeException)
        {
            throw new ScriptRuntimeException($"Failed to construct mission action '{type}' ({actualType.FullName}): {e.Message}");
        }

        var elementObject = new JObject
        {
            ["$type"] = actualType.AssemblyQualifiedName
        };
        foreach (var (key, value) in JObject.FromObject(instance))
        {
            elementObject[key] = value;
        }
        var ud = JsonUserData.GetFromJToken(elementObject);
        callback((JsonUserData)ud.UserData.Object);
        return ud;
    }
    #endregion
}
