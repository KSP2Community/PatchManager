using System;
using System.Collections.Generic;
using HarmonyLib;
using KSP.Modules;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Utility;
using PatchManager.Parts.Attributes;

namespace PatchManager.Parts.UserData;

/// <summary>
/// <c>Data_Engine</c> module-data adapter that exposes the engine's <c>engineModes</c> array as a typed
/// <see cref="ModesUserData" /> rather than a raw <see cref="JsonUserData" />.
/// </summary>
[MoonSharpUserData]
[ModuleDataAdapter(typeof(Data_Engine))]
public class EngineUserData : ExtensibleJsonUserData
{
    private DynValue _modes;

    /// <summary>
    /// Creates the engine adapter and wraps the <c>engineModes</c> array as a typed <see cref="ModesUserData" />.
    /// </summary>
    /// <param name="moduleData">The engine module's <c>DataObject</c> JSON.</param>
    public EngineUserData(JObject moduleData) : base(moduleData)
    {
        _modes = MoonSharp.Interpreter.UserData.Create(new ModesUserData((JArray)Token["engineModes"]));
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "engineModes";
    }

    /// <inheritdoc />
    public override DynValue TryToGet(string property)
    {
        if (property == "engineModes") return _modes;

        return null;
    }

    /// <inheritdoc />
    public override bool TryToSet(string property, DynValue value)
    {

        if (property == "engineModes")
        {
            throw new Exception("Use the relevant Mode methods to update engine modes!");
        }
        return false;
    }

    /// <inheritdoc />
    public override bool TryToRemove(string property)
    {
        throw new Exception("You cannot remove this property.");
    }


}
