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
    /// Registers a celestial-body patch with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// The patch matches every body by default. Restrict it via <see cref="PatchDefinition.Named" />, which supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition Patch(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context,"Planet", "celestial_bodies", name);
    }

    /// <summary>
    /// Registers a patch against the default galaxy definition.
    /// </summary>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchDefaultGalaxy(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context,"Galaxy", "GalaxyDefinition_Default", name);
    }

    /// <summary>
    /// Registers a patch against the <c>atmosphere_overrides</c> label with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// Use <see cref="PatchDefinition.Named" /> to restrict which override files the patch runs against.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchAtmosphereOverride(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context,"JSON", "atmosphere_overrides", name);
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
    /// Registers a patch against the <c>volume_cloud_overrides</c> label with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// Use <see cref="PatchDefinition.Named" /> to restrict which override files the patch runs against.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchCloudOverride(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context,"Cloud", "volume_cloud_overrides", name);
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
