using System;
using KSP.Modules;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;
using PatchManager.LuaPatching;

namespace PatchManager.Parts.UserData;

/// <summary>
/// Indexed-list wrapper for an engine's <c>engineModes</c> array, keyed by each mode's <c>engineID</c>.
/// </summary>
[MoonSharpUserData]
public class ModesUserData : IndexedListUserData
{
    /// <summary>
    /// Creates the wrapper around the engine modes array.
    /// </summary>
    /// <param name="token">The <c>engineModes</c> JSON array.</param>
    public ModesUserData(JArray token) : base(token)
    {
    }

    /// <inheritdoc />
    public override string Name(JToken source)
    {
        return RequireString(source["engineID"], "engineModes[].engineID");
    }

    /// <summary>
    /// Adds a new engine mode with the given <c>engineID</c> and runs <paramref name="callback" /> against it for further configuration.
    /// </summary>
    /// <param name="mode">The new mode's <c>engineID</c>.</param>
    /// <param name="callback">Callback that receives the new mode for further configuration.</param>
    public void Add(string mode, Action<JsonUserData> callback)
    {
        var engineModeData = new Data_Engine.EngineMode()
        {
            engineID = mode
        };
        var json = JObject.FromObject(engineModeData);
        var ud = GetFromJToken(json);
        callback((JsonUserData)ud.UserData.Object);
        Append(ud);
    }
}
