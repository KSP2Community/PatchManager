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
    /// Registers a patch that copies each celestial body it matches under a new name, then runs <paramref name="patchMethod" />
    /// on the copy.
    /// </summary>
    /// <remarks>
    /// Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces the patch name.</param>
    /// <param name="source">The name of the celestial body to copy. Supports <c>*</c> and <c>?</c> wildcards.</param>
    /// <param name="newName">The name of the copy, with <c>{name}</c> standing in for the source's name.</param>
    /// <param name="patchMethod">The callback to run on each copy, or <c>null</c> to copy without changes.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition Duplicate(ScriptExecutionContext context, string source, string newName,
        Func<DynValue, string> patchMethod = null)
    {
        return _core.Duplicate(context, "Planet", "celestial_bodies", source, newName, patchMethod);
    }

    /// <summary>
    /// Registers a patch against the default galaxy definition.
    /// </summary>
    /// <remarks>
    /// Every galaxy shares the galaxy definition label, so the patch is restricted to the stock galaxy and leaves its
    /// copies alone.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchDefaultGalaxy(ScriptExecutionContext context, string name)
    {
        return PatchGalaxy(context, SerializedSavedGame.DEFAULT_GALAXY_DEFINITION_KEY, name);
    }

    /// <summary>
    /// Registers a patch against the galaxy definition with the given key.
    /// </summary>
    /// <remarks>
    /// Every galaxy shares the galaxy definition label, so the patch is restricted to the one asset named by the key.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="galaxyDefinitionKey">The key of the galaxy definition to patch.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchGalaxy(ScriptExecutionContext context, string galaxyDefinitionKey, string name)
    {
        return _core.Patch(context, "Galaxy", GalaxyDefinitionManager.GALAXY_DEFINITION_LABEL, name)
            .Named(galaxyDefinitionKey);
    }

    /// <summary>
    /// Registers a patch that copies the galaxy definition with the given key under a new key, then runs
    /// <paramref name="patchMethod" /> on the copy.
    /// </summary>
    /// <remarks>
    /// The copy joins every other galaxy in the galaxy definition label, so <see cref="PatchGalaxy" /> reaches it by its
    /// new key. Patches ordered after the duplicate that name the source do not reach the copy.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces the patch name.</param>
    /// <param name="galaxyDefinitionKey">The key of the galaxy definition to copy.</param>
    /// <param name="newGalaxyDefinitionKey">The key of the copy.</param>
    /// <param name="patchMethod">The callback to run on the copy, or <c>null</c> to copy without changes.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition DuplicateGalaxy(ScriptExecutionContext context, string galaxyDefinitionKey,
        string newGalaxyDefinitionKey, Func<DynValue, string> patchMethod = null)
    {
        return _core.Duplicate(context, "Galaxy", GalaxyDefinitionManager.GALAXY_DEFINITION_LABEL, galaxyDefinitionKey,
            newGalaxyDefinitionKey, patchMethod);
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
    /// Registers a patch that copies each atmosphere override it matches under a new name, then runs <paramref name="patchMethod" />
    /// on the copy.
    /// </summary>
    /// <remarks>
    /// Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces the patch name.</param>
    /// <param name="source">The name of the atmosphere override to copy. Supports <c>*</c> and <c>?</c> wildcards.</param>
    /// <param name="newName">The name of the copy, with <c>{name}</c> standing in for the source's name.</param>
    /// <param name="patchMethod">The callback to run on each copy, or <c>null</c> to copy without changes.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition DuplicateAtmosphereOverride(ScriptExecutionContext context, string source, string newName,
        Func<DynValue, string> patchMethod = null)
    {
        return _core.Duplicate(context, "JSON", "atmosphere_overrides", source, newName, patchMethod);
    }

    /// <summary>
    /// Creates a new atmosphere override for the given body and runs <paramref name="callback" /> against it
    /// for further configuration.
    /// </summary>
    /// <remarks>
    /// Setting <c>Layer</c> in the callback limits the override to galaxies that take the body from that layer, and
    /// names the override after the layer too, so each layer can have its own.
    /// </remarks>
    /// <param name="name">The body name the override applies to.</param>
    /// <param name="callback">Callback that receives the new override for further configuration.</param>
    public void CreateAtmosphereOverride(string name, Action<JsonUserData> callback)
    {
        var data = new AtmosphereOverride
        {
            PlanetName = name
        };
        var ud = JsonUserData.GetFromJToken(JObject.FromObject(data));
        var json = (JsonUserData)ud.UserData.Object;
        callback(json);
        _core.New("JSON", "atmosphere_overrides", OverrideAssetName("atmosphere_override", name, json.Token), ud);
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
    /// Registers a patch that copies each volume cloud override it matches under a new name, then runs <paramref name="patchMethod" />
    /// on the copy.
    /// </summary>
    /// <remarks>
    /// Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces the patch name.</param>
    /// <param name="source">The name of the volume cloud override to copy. Supports <c>*</c> and <c>?</c> wildcards.</param>
    /// <param name="newName">The name of the copy, with <c>{name}</c> standing in for the source's name.</param>
    /// <param name="patchMethod">The callback to run on each copy, or <c>null</c> to copy without changes.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition DuplicateCloudOverride(ScriptExecutionContext context, string source, string newName,
        Func<DynValue, string> patchMethod = null)
    {
        return _core.Duplicate(context, "Cloud", "volume_cloud_overrides", source, newName, patchMethod);
    }

    /// <summary>
    /// Creates a new volume-cloud override for the given body and runs <paramref name="callback" /> against it
    /// for further configuration.
    /// </summary>
    /// <remarks>
    /// Setting <c>Layer</c> in the callback limits the override to galaxies that take the body from that layer, and
    /// names the override after the layer too, so each layer can have its own.
    /// </remarks>
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
        _core.New("Cloud", "volume_cloud_overrides", OverrideAssetName("volume_cloud_override", name, typed.Token), ud);
    }

    // A default-layer override keeps its plain name, and a layered one adds its layer
    private static string OverrideAssetName(string prefix, string bodyName, JToken overrideToken)
    {
        string layer = overrideToken["Layer"]?.Type == JTokenType.String ? (string)overrideToken["Layer"] : null;
        return CampaignPack.IsDefaultLayer(layer) ? $"{prefix}_{bodyName}" : $"{prefix}_{bodyName}_{layer}";
    }
}
