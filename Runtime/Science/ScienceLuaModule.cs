using System;
using KSP.Game.Science;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Science.UserData;

namespace PatchManager.Science;

/// <summary>
/// Lua submodule exposed as <c>PM.Science</c>, providing patches and creation helpers for science discoverables,
/// experiments, regions, and tech nodes.
/// </summary>
[PatchManagerModule("Science")]
[MoonSharpUserData]
public class ScienceLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    /// <summary>
    /// Creates the submodule bound to the given core and universe.
    /// </summary>
    /// <param name="pmc">The shared <c>PM</c> core instance.</param>
    /// <param name="universe">The owning universe.</param>
    public ScienceLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }

    #region Discoverables
    /// <summary>
    /// Registers a patch that runs against every science region's discoverables list.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the asset, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchAllDiscoverables(Script script, Func<DiscoverablesUserData, string> callback)
    {
        return _core.PatchAll(script, "Discoverables", "science_region_discoverables", callback.ToPatchMethod());
    }

    /// <summary>
    /// Registers a patch that runs against the discoverables list matching <paramref name="name" />.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="name">The discoverables-asset name pattern (supports <c>*</c> and <c>?</c> wildcards).</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the asset, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchDiscoverables(Script script, string name, Func<DiscoverablesUserData, string> callback)
    {
        return _core.Patch(script, "Discoverables", "science_region_discoverables", name, callback.ToPatchMethod());
    }
    #endregion

    #region Experiments

    /// <summary>
    /// Registers a patch that runs against every science experiment.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the experiment, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchAllExperiments(Script script, Func<ExperimentUserData, string> callback)
    {
        return _core.PatchAll(script, "Experiment", "scienceExperiment", callback.ToPatchMethod());
    }

    /// <summary>
    /// Registers a patch that runs against the science experiment matching <paramref name="name" />.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="name">The experiment name pattern (supports <c>*</c> and <c>?</c> wildcards).</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the experiment, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchExperiment(Script script, string name, Func<ExperimentUserData, string> callback)
    {
        return _core.Patch(script, "Experiment", "scienceExperiment", name, callback.ToPatchMethod());
    }

    /// <summary>
    /// Creates a new science experiment with the given name and runs <paramref name="callback" /> against it for
    /// further configuration.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="name">The experiment name.</param>
    /// <param name="callback">Callback that receives the new experiment for further configuration.</param>
    public void NewExperiment(Script script, string name, Action<ExperimentUserData> callback)
    {
        var core = new ExperimentCore
        {
            data = new ExperimentDefinition
            {
                ExperimentID = name
            }
        };
        var typed = new ExperimentUserData(JObject.FromObject(core));
        var ud = MoonSharp.Interpreter.UserData.Create(typed);
        callback(typed);
        _core.New("Experiment", "scienceExperiment", name, ud);
    }
    #endregion

    #region Science Regions
    /// <summary>
    /// Registers a patch that runs against every science-region asset.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the asset, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchAllRegions(Script script, Func<ScienceRegionsUserData, string> callback)
    {
        return _core.PatchAll(script, "ScienceRegions", "science_region", callback.ToPatchMethod());
    }

    /// <summary>
    /// Registers a patch that runs against the science-region asset matching <paramref name="name" />.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="name">The region-asset name pattern (supports <c>*</c> and <c>?</c> wildcards).</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the asset, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchRegions(Script script, string name, Func<ScienceRegionsUserData, string> callback)
    {
        return _core.Patch(script, "ScienceRegions", "science_region", name, callback.ToPatchMethod());
    }
    #endregion

    #region Tech Nodes

    /// <summary>
    /// Registers a JSON patch that runs against every tech-tree node.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the node, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchAllTechNodes(Script script, Func<JsonUserData, string> callback)
    {
        return _core.PatchAll(script, "JSON", "techNodeData", callback.ToPatchMethod());
    }

    /// <summary>
    /// Registers a JSON patch that runs against the tech-tree node matching <paramref name="name" />.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="name">The tech node name pattern (supports <c>*</c> and <c>?</c> wildcards).</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the node, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchTechNode(Script script, string name, Func<JsonUserData, string> callback)
    {
        return _core.Patch(script, "JSON", "techNodeData", name, callback.ToPatchMethod());
    }

    /// <summary>
    /// Adds the given part IDs to the tech node named <paramref name="nodeName" />'s <c>UnlockedPartIds</c> list.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="nodeName">The tech node name.</param>
    /// <param name="parts">The part IDs to append.</param>
    public void AddPartsToTechNode(Script script, string nodeName, params string[] parts)
    {
        PatchTechNode(script, nodeName, node =>
        {
            var array = (JArray)node.Token["UnlockedPartsIDs"];
            foreach (var part in parts)
            {
                array.Add(part);
            }
            return null;
        });
    }

    #endregion
}
