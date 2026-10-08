using System;
using System.Collections.Generic;
using KSP.Sim;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Planets.Overrides;
using PatchManager.Planets.UserData;
using Redux.Packs;

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
        return _core.Patch(context, "Planet", "celestial_bodies", name);
    }

    /// <summary>
    /// Registers a patch against the default galaxy definition.
    /// </summary>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchDefaultGalaxy(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context, "Galaxy", "GalaxyDefinition_Default", name);
    }

    /// <summary>
    /// Registers a patch against the galaxy definition with the given key.
    /// </summary>
    /// <remarks>
    /// The stock galaxy is only reachable by its own key. Every other galaxy shares the galaxy definition label, so the
    /// patch is restricted to the one asset named by the key.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="galaxyDefinitionKey">The key of the galaxy definition to patch.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchGalaxy(ScriptExecutionContext context, string galaxyDefinitionKey, string name)
    {
        return galaxyDefinitionKey == SerializedSavedGame.DEFAULT_GALAXY_DEFINITION_KEY
            ? _core.Patch(context, "Galaxy", galaxyDefinitionKey, name)
            : _core.Patch(context, "Galaxy", GalaxyDefinitionManager.GALAXY_DEFINITION_LABEL, name)
                .Named(galaxyDefinitionKey);
    }

    /// <summary>
    /// Creates a new galaxy definition with no bodies under the given key and runs <paramref name="callback" />
    /// against it for further configuration.
    /// </summary>
    /// <param name="galaxyDefinitionKey">The key that saves and campaign packs load the galaxy definition by.</param>
    /// <param name="callback">Callback that receives the new galaxy definition for further configuration.</param>
    public void CreateGalaxy(string galaxyDefinitionKey, Action<GalaxyUserData> callback)
    {
        var data = new SerializedGalaxyDefinition
        {
            Name = galaxyDefinitionKey,
            Version = "0.0.1",
            CelestialBodies = new List<SerializedCelestialBody>()
        };
        var typed = new GalaxyUserData(JObject.FromObject(data));
        var ud = MoonSharp.Interpreter.UserData.Create(typed);
        callback(typed);
        _core.New("Galaxy", GalaxyDefinitionManager.GALAXY_DEFINITION_LABEL, galaxyDefinitionKey, ud);
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
        return _core.Patch(context, "JSON", "atmosphere_overrides", name);
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
        return _core.Patch(context, "Cloud", "volume_cloud_overrides", name);
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
