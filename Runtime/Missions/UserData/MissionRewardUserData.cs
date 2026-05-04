using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Missions.UserData;

/// <summary>
/// Indexed-list wrapper for a stage's <c>MissionRewardDefinitions</c> array, keyed by each reward's <c>MissionRewardType</c>.
/// </summary>
[MoonSharpUserData]
public class MissionRewardUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper around the mission reward, exposing its <c>MissionRewardDefinitions</c> array.
    /// </summary>
    /// <param name="token">The mission reward JSON (containing a <c>MissionRewardDefinitions</c> array).</param>
    public MissionRewardUserData(JToken token) : base((JArray)token["MissionRewardDefinitions"])
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return source["MissionRewardType"].Value<string>();
    }
}
