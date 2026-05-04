using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Planets.UserData;

/// <summary>
/// Volume-cloud configuration wrapper that exposes the <c>cumulusList</c> array as a typed
/// <see cref="CloudUserData" /> rather than a raw <see cref="PatchManager.LuaPatching.JsonUserData" />.
/// </summary>
[MoonSharpUserData]
public class VolumeCloudUserData : ExtensibleJsonUserData
{
    /// <summary>
    /// Creates the wrapper around the volume-cloud configuration JSON.
    /// </summary>
    /// <param name="token">The volume-cloud configuration JSON.</param>
    public VolumeCloudUserData(JToken token) : base(token)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetExtraAndOverriddenKeys()
    {
        yield return "cumulusList";
    }

    /// <inheritdoc />
    public override DynValue TryToGet(string property)
    {
        if (property == "cumulusList")
        {
            return MoonSharp.Interpreter.UserData.Create(new CloudUserData((JArray)Token["cumulusList"]));
        }

        return null;
    }

    /// <inheritdoc />
    public override bool TryToSet(string property, DynValue value)
    {
        if (property == "cumulusList")
        {
            throw new Exception("You cannot set this property.");
        }
        return false;
    }

    /// <inheritdoc />
    public override bool TryToRemove(string property)
    {
        if (property == "cumulusList")
        {
            throw new Exception("You cannot remove this property.");
        }

        return false;
    }
}
