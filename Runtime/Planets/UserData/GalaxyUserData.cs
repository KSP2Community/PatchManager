using System;
using System.Collections.Generic;
using KSP.Sim;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Planets.UserData;

[MoonSharpUserData]
public class GalaxyUserData : ExtensibleJsonUserData
{
    public GalaxyUserData(JToken token) : base(token)
    {
    }

    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        foreach (var body in (JArray)Token["CelestialBodies"])
        {
            yield return body["GUID"].Value<string>();
        }
    }

    public override DynValue TryToGet(string property)
    {
        foreach (var body in (JArray)Token["CelestialBodies"])
        {
            if (body["GUID"].Value<string>() == property)
            {
                return GetFromJToken(body);
            }
        }

        return null;
    }

    public override bool TryToSet(string property, DynValue value)
    {
        foreach (var body in (JArray)Token["CelestialBodies"])
        {
            if (body["GUID"].Value<string>() == property)
            {
                throw new Exception("Cannot set this property, use the methods for patching/removing");
            }
        }
        return false;
    }

    public override bool TryToRemove(string property)
    {
        var index = 0;
        var found = false;
        foreach (var body in (JArray)Token["CelestialBodies"])
        {
            if (body["GUID"].Value<string>() == property)
            {
                found = true;
                break;
            }
            index++;
        }

        if (found)
        {
            ((JArray)Token["CelestialBodies"]).RemoveAt(index);
            return true;
        }
        throw new Exception("Cannot remove this property.");
    }

    public void Add(string planetName, Action<JsonUserData> callback)
    {
        var obj = new SerializedCelestialBody
        {
            GUID = planetName,
            OrbitProperties = new SerializedOrbitProperties(),
            OrbiterProperties = new SerializedOribiterDefinition()
        };
        var jToken = JObject.FromObject(obj);
        var ud = GetFromJToken(jToken);
        ((JArray)Token["CelestialBodies"]).Add(jToken);
        callback((JsonUserData)ud.UserData.Object);
    }
}