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
    /// Registers a discoverables-list patch with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// The patch matches every discoverables asset by default. Restrict it via <see cref="PatchDefinition.Named" />, which supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchDiscoverables(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context,"Discoverables", "science_region_discoverables", name);
    }
    #endregion

    #region Experiments

    /// <summary>
    /// Registers a science-experiment patch with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// The patch matches every experiment by default. Restrict it via <see cref="PatchDefinition.Named" />, which supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchExperiments(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context,"Experiment", "scienceExperiment", name);
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
    /// Registers a science-region patch with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// The patch matches every region asset by default. Restrict it via <see cref="PatchDefinition.Named" />, which supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchRegions(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context,"ScienceRegions", "science_region", name);
    }
    #endregion

    #region Tech Nodes


    /// <summary>
    /// Registers a tech-tree-node patch with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// The patch matches every tech-tree node by default. Restrict it via <see cref="PatchDefinition.Named" />, which supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition PatchTechNodes(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context,"JSON", "techNodeData", name);
    }

    /// <summary>
    /// Adds the given part IDs to the tech node named <paramref name="nodeName" />'s <c>UnlockedPartIds</c> list.
    /// </summary>
    /// <param name="context">The Lua execution context.</param>
    /// <param name="nodeName">The tech node name.</param>
    /// <param name="parts">The part IDs to append.</param>
    public void AddPartsToTechNode(ScriptExecutionContext context, string nodeName, params string[] parts)
    {
        PatchTechNodes(context, $"add_{Guid.NewGuid()}").Named(nodeName).Do(node =>
        {
            if (node.IsNil()) return null;

            var array = JsonUserData.RequireArray(((JsonUserData)node.UserData.Object).Token["UnlockedPartsIDs"], "UnlockedPartsIDs");
            foreach (var part in parts)
            {
                array.Add(part);
            }
            return null;
        });
    }

    #endregion
}
