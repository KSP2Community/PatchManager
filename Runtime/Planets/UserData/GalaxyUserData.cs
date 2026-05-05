using System;
using System.Collections.Generic;
using KSP.Sim;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Planets.UserData;

/// <summary>
/// Galaxy definition wrapper that exposes each celestial body in the galaxy's <c>CelestialBodies</c> array as a
/// virtual property keyed by GUID.
/// </summary>
[MoonSharpUserData]
public class GalaxyUserData : ExtensibleJsonUserData
{
    /// <summary>
    /// Creates the wrapper around the galaxy JSON.
    /// </summary>
    /// <param name="token">The galaxy definition JSON.</param>
    public GalaxyUserData(JToken token) : base(token)
    {
    }

    private JArray Bodies => RequireArray(Token["CelestialBodies"], "CelestialBodies");

    /// <inheritdoc />
    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        foreach (var body in Bodies)
        {
            yield return RequireString(body["GUID"], "CelestialBodies[].GUID");
        }
    }

    /// <inheritdoc />
    public override DynValue TryToGet(string property)
    {
        foreach (var body in Bodies)
        {
            if (RequireString(body["GUID"], "CelestialBodies[].GUID") == property)
            {
                return GetFromJToken(body);
            }
        }

        return null;
    }

    /// <inheritdoc />
    public override bool TryToSet(string property, DynValue value)
    {
        foreach (var body in Bodies)
        {
            if (RequireString(body["GUID"], "CelestialBodies[].GUID") == property)
            {
                throw new ScriptRuntimeException("Cannot set this property, use the methods for patching/removing");
            }
        }
        return false;
    }

    /// <inheritdoc />
    public override bool TryToRemove(string property)
    {
        var bodies = Bodies;
        var index = 0;
        var found = false;
        foreach (var body in bodies)
        {
            if (RequireString(body["GUID"], "CelestialBodies[].GUID") == property)
            {
                found = true;
                break;
            }
            index++;
        }

        if (found)
        {
            bodies.RemoveAt(index);
            return true;
        }
        throw new ScriptRuntimeException("Cannot remove this property.");
    }

    /// <summary>
    /// Adds a new celestial body with the given GUID to the galaxy and runs <paramref name="callback" /> against
    /// it for further configuration.
    /// </summary>
    /// <param name="planetName">The new body's GUID.</param>
    /// <param name="callback">Callback that receives the new body for further configuration.</param>
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
        Bodies.Add(jToken);
        callback((JsonUserData)ud.UserData.Object);
    }
}
