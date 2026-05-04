using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Missions.UserData;

[MoonSharpUserData]
public class StagesUserData : IndexedListUserData
{
    public StagesUserData(JArray token) : base(token)
    {
    }

    public override string Name(JToken source)
    {
        return source["name"]!.Value<string>();
    }

    public override DynValue Convert(JToken source)
    {
        return MoonSharp.Interpreter.UserData.Create(new StageUserData(source));
    }
}