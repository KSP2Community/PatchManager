using System;
using KSP.Modules;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;
using PatchManager.LuaPatching;

namespace PatchManager.Parts.UserData;

[MoonSharpUserData]
public class ModesUserData : IndexedListUserData
{
    public ModesUserData(JArray token) : base(token)
    {
    }

    public override string Name(JToken source)
    {
        return source["engineID"].Value<string>();
    }
    
    public void Add(string mode, Action<JsonUserData> callback)
    {
        var engineModeData = new Data_Engine.EngineMode()
        {
            engineID = mode
        };
        var json = JObject.FromObject(engineModeData);
        var ud = GetFromJToken(json);
        callback((JsonUserData)ud.UserData.Object);
        Append(ud);
    }
}