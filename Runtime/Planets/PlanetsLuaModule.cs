using System;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Planets.Overrides;
using PatchManager.Planets.UserData;

namespace PatchManager.Planets;

[PatchManagerModule("Planets")]
[MoonSharpUserData]
public class PlanetsLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    public PlanetsLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }

    public LuaPatch PatchAll(Script script, Func<CelestialBodyUserData, string> callback)
    {
        return _core.PatchAll(script, "Planet", "celestial_bodies", callback.ToPatchMethod());
    }

    public LuaPatch Patch(Script script, string name, Func<CelestialBodyUserData, string> callback)
    {
        return _core.Patch(script, "Planet", "celestial_bodies", name, callback.ToPatchMethod());
    }

    public LuaPatch PatchDefaultGalaxy(Script script, Func<GalaxyUserData, string> callback)
    {
        return _core.Patch(script, "Galaxy", "GalaxyDefinition_Default", "GalaxyDefinition_Default", callback.ToPatchMethod());
    }

    public LuaPatch PatchAtmosphereOverride(Script script, string name, Func<JsonUserData, string> callback)
    {
        return _core.Patch(script, "JSON", "atmosphere_overrides", $"atmosphere_override_{name}", callback.ToPatchMethod());
    }

    public void CreateAtmosphereOverride(string name, Action<JsonUserData> callback)
    {
        var data = new AtmosphereOverride
        {
            PlanetName = name
        };
        var ud = JsonUserData.GetFromJToken(JObject.FromObject(data));
        _core.New("JSON", "atmosphere_overrides", $"atmosphere_override_{name}", ud);
        callback((JsonUserData)ud.UserData.Object);
    }

    public LuaPatch PatchCloudOverride(Script script, string name, Func<VolumeCloudUserData, string> callback)
    {
        return _core.Patch(script, "Cloud", "volume_cloud_overrides", $"volume_cloud_override_{name}", callback.ToPatchMethod());
    }

    public void CreateCloudOverride(string name, Action<VolumeCloudUserData> callback)
    {
        var data = new VolumeCloudConfigurationOverride
        {
            bodyName = name
        };
        var typed = new VolumeCloudUserData(JObject.FromObject(data));
        var ud = MoonSharp.Interpreter.UserData.Create(typed);
        callback(typed);
        _core.New("Cloud", "volume_cloud_overrides", $"volume_cloud_override_{name}", ud);
    }
}
