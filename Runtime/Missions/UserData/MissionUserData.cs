using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Missions.UserData;

/// <summary>
/// Mission definition wrapper exposing the <c>missionStages</c> array as a typed <see cref="StagesUserData" /> and
/// the optional <c>ContentBranches</c> array as a typed <see cref="ContentBranchesUserData" />.
/// </summary>
[MoonSharpUserData]
public class MissionUserData : ExtensibleJsonUserData
{
    private DynValue _stages;
    [CanBeNull] private DynValue _contentBranches;

    /// <summary>
    /// Creates the wrapper around the mission JSON, seeding the typed stages and content-branches caches.
    /// </summary>
    /// <param name="token">The mission definition JSON.</param>
    public MissionUserData(JToken token) : base(token)
    {
        _stages = MoonSharp.Interpreter.UserData.Create(new StagesUserData((JArray)token["missionStages"]));
        RefreshContentBranches();
    }

    private void RefreshContentBranches()
    {
        if (((JObject)Token).TryGetValue("ContentBranches", out var branches))
        {
            if (branches.Type != JTokenType.Array)
            {
                _contentBranches = null;
            }
            else
            {
                _contentBranches =
                    MoonSharp.Interpreter.UserData.Create(
                        new ContentBranchesUserData((JArray)branches));
            }
        }
        else
        {
            _contentBranches = null;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "missionStages";
        yield return "ContentBranches";
    }

    /// <inheritdoc />
    public override DynValue TryToGet(string property)
    {
        if (property == "missionStages") return _stages;
        if (property == "ContentBranches" && _contentBranches != null) return _contentBranches;
        return null;
    }

    /// <inheritdoc />
    public override bool TryToSet(string property, DynValue value)
    {
        if (property is "missionStages") throw new Exception("You cannot set this property.");
        else if (property == "ContentBranches")
        {
            Token["ContentBranches"] = GetJTokenForDynValue(new JArray(), value);
            RefreshContentBranches();
        }

        return false;
    }

    /// <inheritdoc />
    public override bool TryToRemove(string property)
    {
        if (property == "ContentBranches")
        {
            ((JObject)Token).Remove("ContentBranches");
            RefreshContentBranches();
        }

        throw new Exception("You cannot remove this property.");
    }
}
