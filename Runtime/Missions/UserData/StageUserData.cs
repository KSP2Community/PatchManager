using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Missions.UserData;

[MoonSharpUserData]
public class StageUserData : ExtensibleJsonUserData
{
    public StageUserData(JToken token) : base(token)
    {
    }

    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "MissionReward";
    }

    public override DynValue TryToGet(string property)
    {
        if (property == "MissionReward")
        {
            if (((JObject)Token).TryGetValue(property, out var value))
            {
                return MoonSharp.Interpreter.UserData.Create(new MissionRewardUserData(value));
            }
        }

        return null;
    }

    public override bool TryToSet(string property, DynValue value)
    {
        return false;
    }

    public override bool TryToRemove(string property)
    {
        return false;
    }
}