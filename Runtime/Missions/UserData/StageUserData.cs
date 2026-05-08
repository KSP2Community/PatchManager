using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Missions.UserData;

/// <summary>
/// Mission stage wrapper exposing the optional <c>MissionReward</c> object as a typed <see cref="MissionRewardUserData" />.
/// </summary>
[MoonSharpUserData]
public class StageUserData : ExtensibleJsonUserData
{
    /// <summary>
    /// Creates the wrapper around the mission stage JSON.
    /// </summary>
    /// <param name="token">The mission stage JSON.</param>
    public StageUserData(JToken token) : base(token)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "MissionReward";
    }

    /// <inheritdoc />
    public override DynValue TryToGet(string property)
    {
        if (property == "MissionReward")
        {
            if (RequireObject(Token, "missionStage").TryGetValue(property, out var value))
            {
                return MoonSharp.Interpreter.UserData.Create(new MissionRewardUserData(value));
            }
        }

        return null;
    }

    /// <inheritdoc />
    public override bool TryToSet(string property, DynValue value)
    {
        return false;
    }

    /// <inheritdoc />
    public override bool TryToRemove(string property)
    {
        return false;
    }
}
