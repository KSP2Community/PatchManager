using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Missions.UserData;

[MoonSharpUserData]
public class ContentBranchesUserData : IndexedListUserData
{
    public ContentBranchesUserData(JArray token) : base(token)
    {
    }

    public override string Name(JToken source)
    {
        return source["ID"].Value<string>();
    }
}