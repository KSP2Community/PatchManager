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

/// <summary>
/// Lua submodule exposed as <c>PM.Planets</c>, providing patches for celestial bodies, the default galaxy,
/// and atmosphere / cloud overrides.
/// </summary>
[PatchManagerModule("Planets")]
[MoonSharpUserData]
public class PlanetsLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    /// <summary>
    /// Creates the submodule bound to the given core and universe.
    /// </summary>
    /// <param name="pmc">The shared <c>PM</c> core instance.</param>
    /// <param name="universe">The owning universe.</param>
    public PlanetsLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }

    /// <summary>
    /// Registers a patch that runs against every celestial body.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the body, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchAll(Script script, Func<CelestialBodyUserData, string> callback)
    {
        return _core.PatchAll(script, "Planet", "celestial_bodies", callback.ToPatchMethod());
    }

    /// <summary>
    /// Registers a patch that runs against the celestial body matching <paramref name="name" />.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="name">The body name pattern (supports <c>*</c> and <c>?</c> wildcards).</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the body, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch Patch(Script script, string name, Func<CelestialBodyUserData, string> callback)
    {
        return _core.Patch(script, "Planet", "celestial_bodies", name, callback.ToPatchMethod());
    }

    /// <summary>
    /// Registers a patch that runs against the default galaxy definition.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the galaxy, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchDefaultGalaxy(Script script, Func<GalaxyUserData, string> callback)
    {
        return _core.Patch(script, "Galaxy", "GalaxyDefinition_Default", "GalaxyDefinition_Default", callback.ToPatchMethod());
    }

    /// <summary>
    /// Registers a patch that runs against the atmosphere override for <paramref name="name" />.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="name">The body name whose atmosphere override to patch.</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the override, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchAtmosphereOverride(Script script, string name, Func<JsonUserData, string> callback)
    {
        return _core.Patch(script, "JSON", "atmosphere_overrides", $"atmosphere_override_{name}", callback.ToPatchMethod());
    }

    /// <summary>
    /// Creates a new atmosphere override for the given body and runs <paramref name="callback" /> against it
    /// for further configuration.
    /// </summary>
    /// <param name="name">The body name the override applies to.</param>
    /// <param name="callback">Callback that receives the new override for further configuration.</param>
    public void CreateAtmosphereOverride(string name, Action<JsonUserData> callback)
    {
        var data = new AtmosphereOverride
        {
            PlanetName = name
        };
        var ud = JsonUserData.GetFromJToken(JObject.FromObject(data));
        callback((JsonUserData)ud.UserData.Object);
        _core.New("JSON", "atmosphere_overrides", $"atmosphere_override_{name}", ud);
    }

    /// <summary>
    /// Registers a patch that runs against the volume-cloud override for <paramref name="name" />.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="name">The body name whose cloud override to patch.</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the override, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchCloudOverride(Script script, string name, Func<VolumeCloudUserData, string> callback)
    {
        return _core.Patch(script, "Cloud", "volume_cloud_overrides", $"volume_cloud_override_{name}", callback.ToPatchMethod());
    }

    /// <summary>
    /// Creates a new volume-cloud override for the given body and runs <paramref name="callback" /> against it
    /// for further configuration.
    /// </summary>
    /// <param name="name">The body name the override applies to.</param>
    /// <param name="callback">Callback that receives the new override for further configuration.</param>
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
