using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Planets.UserData;

[MoonSharpUserData]
public class CloudUserData : IndexedListUserData
{
    public CloudUserData(JArray token) : base(token)
    {
    }

    public override string Name(JToken source)
    {
        return source["layerName"].Value<string>();
    }
}