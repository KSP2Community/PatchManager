using System;
using System.Collections.Generic;
using HarmonyLib;
using KSP.Modules;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;
using PatchManager.Parts.Attributes;

namespace PatchManager.Parts.UserData;

[MoonSharpUserData]
[ModuleDataAdapter(typeof(Data_Engine))]
public class EngineUserData : ExtensibleJsonUserData
{
    private DynValue _modes;
    public EngineUserData(JObject moduleData) : base(moduleData)
    {
        _modes = MoonSharp.Interpreter.UserData.Create(new ModesUserData((JArray)Token["engineModes"]));
    }
    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "engineModes";
    }

    public override DynValue TryToGet(string property)
    {
        if (property == "engineModes") return _modes;

        return null;
    }

    public override bool TryToSet(string property, DynValue value)
    {

        if (property == "engineModes")
        {
            throw new Exception("Use the relevant Mode methods to update engine modes!");
        }
        return false;
    }

    public override bool TryToRemove(string property)
    {
        throw new Exception("You cannot remove this property.");
    }

    
}