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

[PatchManagerModule("Missions")]
[MoonSharpUserData]
public class MissionsLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    public MissionsLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }

    public LuaPatch PatchAll(Script script, Func<MissionUserData, string> callback)
    {
        return _core.PatchAll(script, "Mission", "missions", callback.ToPatchMethod());
    }

    public LuaPatch Patch(Script script, string name, Func<MissionUserData, string> callback)
    {
        return _core.Patch(script, "Mission", "missions", name, callback.ToPatchMethod());
    }

    #region Utility Methods
    public string GetPropertyWatcher(string name)
        => MissionsTypes.PropertyWatchers[name].AssemblyQualifiedName;

    public string GetMessage(string name)
        => MissionsTypes.Messages[name].AssemblyQualifiedName;
    #endregion

    #region Creation
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

    public DynValue And(CallbackArguments arguments)
    {
        var conditionSet = new ConditionSet
        {
            ConditionMode = LogicalOperator.AND
        };
        var obj = JObject.FromObject(conditionSet);
        foreach (var arg in arguments.GetArray())
        {
            ((JArray)obj["Children"]).Add(JsonUserData.GetJTokenForDynValue(arg));
        }
        return JsonUserData.GetFromJToken(obj);
    }

    public DynValue Or(CallbackArguments arguments)
    {
        var conditionSet = new ConditionSet
        {
            ConditionMode = LogicalOperator.OR
        };
        var obj = JObject.FromObject(conditionSet);
        foreach (var arg in arguments.GetArray())
        {
            ((JArray)obj["Children"]).Add(JsonUserData.GetJTokenForDynValue(arg));
        }
        return JsonUserData.GetFromJToken(obj);
    }

    public DynValue Not(DynValue condition)
    {
        var conditionSet = new ConditionSet
        {
            ConditionMode = LogicalOperator.NOT
        };
        var obj = JObject.FromObject(conditionSet);
        ((JArray)obj["Children"]).Add(JsonUserData.GetJTokenForDynValue(condition));
        return JsonUserData.GetFromJToken(obj);
    }

    public DynValue Action(string type, Action<JsonUserData> callback)
    {
        var actualType = MissionsTypes.Actions[type];
        var elementObject = new JObject
        {
            ["$type"] = actualType.AssemblyQualifiedName
        };
        foreach (var (key, value) in JObject.FromObject(Activator.CreateInstance(actualType)))
        {
            elementObject[key] = value;
        }
        var ud = JsonUserData.GetFromJToken(elementObject);
        callback((JsonUserData)ud.UserData.Object);
        return ud;
    }
    #endregion
}
