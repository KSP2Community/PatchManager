using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Missions.UserData;

[MoonSharpUserData]
public class MissionRewardUserData : IndexedListUserData
{
    public MissionRewardUserData(JToken token) : base((JArray)token["MissionRewardDefinitions"])
    {
    }

    public override string Name(JToken source)
    {
        return source["MissionRewardType"].Value<string>();
    }
}