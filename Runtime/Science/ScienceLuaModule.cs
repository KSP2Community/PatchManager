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

[PatchManagerModule("Science")]
[MoonSharpUserData]
public class ScienceLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    public ScienceLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }

    #region Discoverables
    public LuaPatch PatchAllDiscoverables(Script script, Func<DiscoverablesUserData, string> callback)
    {
        return _core.PatchAll(script, "Discoverables", "science_region_discoverables", callback.ToPatchMethod());
    }

    public LuaPatch PatchDiscoverables(Script script, string name, Func<DiscoverablesUserData, string> callback)
    {
        return _core.Patch(script, "Discoverables", "science_region_discoverables", name, callback.ToPatchMethod());
    }
    #endregion

    #region Experiments

    public LuaPatch PatchAllExperiments(Script script, Func<ExperimentUserData, string> callback)
    {
        return _core.PatchAll(script, "Experiment", "scienceExperiment", callback.ToPatchMethod());
    }

    public LuaPatch PatchExperiment(Script script, string name, Func<ExperimentUserData, string> callback)
    {
        return _core.Patch(script, "Experiment", "scienceExperiment", name, callback.ToPatchMethod());
    }

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
    public LuaPatch PatchAllRegions(Script script, Func<ScienceRegionsUserData, string> callback)
    {
        return _core.PatchAll(script, "ScienceRegions", "science_region", callback.ToPatchMethod());
    }

    public LuaPatch PatchRegions(Script script, string name, Func<ScienceRegionsUserData, string> callback)
    {
        return _core.Patch(script, "ScienceRegions", "science_region", name, callback.ToPatchMethod());
    }
    #endregion

    #region Tech Nodes

    public LuaPatch PatchAllTechNodes(Script script, Func<JsonUserData, string> callback)
    {
        return _core.PatchAll(script, "JSON", "techNodeData", callback.ToPatchMethod());
    }

    public LuaPatch PatchTechNode(Script script, string name, Func<JsonUserData, string> callback)
    {
        return _core.Patch(script, "JSON", "techNodeData", name, callback.ToPatchMethod());
    }

    public void AddPartsToTechNode(Script script, string nodeName, params string[] parts)
    {
        PatchTechNode(script, nodeName, node =>
        {
            var array = (JArray)node.Token["UnlockedPartIds"];
            foreach (var part in parts)
            {
                array.Add(part);
            }
            return null;
        });
    }

    #endregion
}
