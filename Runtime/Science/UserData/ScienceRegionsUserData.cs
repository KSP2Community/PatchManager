using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Science.UserData;

[MoonSharpUserData]
public class ScienceRegionsUserData : IndexedListUserData
{
    public JToken FullToken;
    
    public ScienceRegionsUserData(JToken token) : base((JArray)token["Regions"])
    {
    }


    public string BodyName { get => FullToken["BodyName"].Value<string>(); }
    
    public DynValue SituationData { 
        get => GetFromJToken(FullToken["SituationData"]);
        set => FullToken["SituationData"] = GetJTokenForDynValue(value);
    }
    
    public override string Name(JToken source)
    {
        return source["id"].Value<string>();
    }
}