using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Science.UserData;

[MoonSharpUserData]
public class DiscoverablesUserData : IndexedListUserData
{
    public JToken FullToken;
    
    public DiscoverablesUserData(JToken token) : base((JArray)token["Discoverables"])
    {
    }


    public string BodyName { get => FullToken["BodyName"].Value<string>(); }
    
    public override string Name(JToken source)
    {
        return source["ScienceRegionId"].Value<string>();
    }
}