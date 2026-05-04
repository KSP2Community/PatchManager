using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Missions.UserData;

/// <summary>
/// Indexed-list wrapper for a mission's <c>missionStages</c> array, keyed by each stage's <c>name</c>, wrapping
/// each entry in a typed <see cref="StageUserData" />.
/// </summary>
[MoonSharpUserData]
public class StagesUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper around the mission-stages array.
    /// </summary>
    /// <param name="token">The <c>missionStages</c> JSON array.</param>
    public StagesUserData(JArray token) : base(token)
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return source["name"]!.Value<string>();
    }

    /// <inheritdoc />
    public override DynValue Convert(JToken source)
    {
        return MoonSharp.Interpreter.UserData.Create(new StageUserData(source));
    }
}
