using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Planets.UserData;

public class VolumeCloudUserData : ExtensibleJsonUserData
{
    public VolumeCloudUserData(JToken token) : base(token)
    {
    }

    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "cumulusList";
    }

    public override DynValue TryToGet(string property)
    {
        if (property == "cumulusList")
        {
            return MoonSharp.Interpreter.UserData.Create(new CloudUserData((JArray)Token["cumulusList"]));
        }

        return null;
    }

    public override bool TryToSet(string property, DynValue value)
    {
        if (property == "cumulusList")
        {
            throw new Exception("You cannot set this property.");
        }
        return false;
    }

    public override bool TryToRemove(string property)
    {
        if (property == "cumulusList")
        {
            throw new Exception("You cannot remove this property.");
        }

        return false;
    }
}