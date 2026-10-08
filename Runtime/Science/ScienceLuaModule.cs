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
        return _core.Patch(context, "Discoverables", "science_region_discoverables", name);
    }

    /// <summary>
    /// Registers a patch that copies each discoverables set it matches under a new name, then runs <paramref name="patchMethod" />
    /// on the copy.
    /// </summary>
    /// <remarks>
    /// Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces the patch name.</param>
    /// <param name="source">The name of the discoverables set to copy. Supports <c>*</c> and <c>?</c> wildcards.</param>
    /// <param name="newName">The name of the copy, with <c>{name}</c> standing in for the source's name.</param>
    /// <param name="patchMethod">The callback to run on each copy, or <c>null</c> to copy without changes.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition DuplicateDiscoverables(ScriptExecutionContext context, string source, string newName,
        Func<DynValue, string> patchMethod = null)
    {
        return _core.Duplicate(context, "Discoverables", "science_region_discoverables", source, newName, patchMethod);
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
        return _core.Patch(context, "Experiment", "scienceExperiment", name);
    }

    /// <summary>
    /// Registers a patch that copies each experiment it matches under a new name, then runs <paramref name="patchMethod" />
    /// on the copy.
    /// </summary>
    /// <remarks>
    /// Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces the patch name.</param>
    /// <param name="source">The name of the experiment to copy. Supports <c>*</c> and <c>?</c> wildcards.</param>
    /// <param name="newName">The name of the copy, with <c>{name}</c> standing in for the source's name.</param>
    /// <param name="patchMethod">The callback to run on each copy, or <c>null</c> to copy without changes.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition DuplicateExperiments(ScriptExecutionContext context, string source, string newName,
        Func<DynValue, string> patchMethod = null)
    {
        return _core.Duplicate(context, "Experiment", "scienceExperiment", source, newName, patchMethod);
    }

    /// <summary>
    /// Creates a new science experiment with the given name and runs <paramref name="callback" /> against it for
    /// further configuration.
    /// </summary>
    /// <param name="name">The experiment name.</param>
    /// <param name="callback">Callback that receives the new experiment for further configuration.</param>
    public void NewExperiment(string name, Action<ExperimentUserData> callback)
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
        return _core.Patch(context, "ScienceRegions", "science_region", name);
    }

    /// <summary>
    /// Registers a patch that copies each science region set it matches under a new name, then runs <paramref name="patchMethod" />
    /// on the copy.
    /// </summary>
    /// <remarks>
    /// Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces the patch name.</param>
    /// <param name="source">The name of the science region set to copy. Supports <c>*</c> and <c>?</c> wildcards.</param>
    /// <param name="newName">The name of the copy, with <c>{name}</c> standing in for the source's name.</param>
    /// <param name="patchMethod">The callback to run on each copy, or <c>null</c> to copy without changes.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition DuplicateRegions(ScriptExecutionContext context, string source, string newName,
        Func<DynValue, string> patchMethod = null)
    {
        return _core.Duplicate(context, "ScienceRegions", "science_region", source, newName, patchMethod);
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
        return _core.Patch(context, "JSON", "techNodeData", name);
    }

    /// <summary>
    /// Registers a patch that copies each tech node it matches under a new name, then runs <paramref name="patchMethod" />
    /// on the copy.
    /// </summary>
    /// <remarks>
    /// Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces the patch name.</param>
    /// <param name="source">The name of the tech node to copy. Supports <c>*</c> and <c>?</c> wildcards.</param>
    /// <param name="newName">The name of the copy, with <c>{name}</c> standing in for the source's name.</param>
    /// <param name="patchMethod">The callback to run on each copy, or <c>null</c> to copy without changes.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition DuplicateTechNodes(ScriptExecutionContext context, string source, string newName,
        Func<DynValue, string> patchMethod = null)
    {
        return _core.Duplicate(context, "JSON", "techNodeData", source, newName, patchMethod);
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
