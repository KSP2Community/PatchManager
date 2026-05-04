using System.Collections.Generic;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using UniLinq;

namespace PatchManager.LuaPatching.Utility;

/// <summary>
/// An extensible json adapter for user data
/// Useful for overriding/adding properties to basic json data
/// </summary>
public abstract class ExtensibleJsonUserData : JsonUserData
{
    /// <summary>
    /// Create the extensible user data
    /// </summary>
    /// <param name="token">The token to wrap around</param>
    protected ExtensibleJsonUserData(JToken token) : base(token)
    {
    }

    /// <summary>
    /// What extra/overridden keys are added to this user data?
    /// </summary>
    /// <returns>The extra and overridden keys on this user data</returns>
    [MoonSharpHidden]
    public abstract IEnumerable<string> GetExtraAndOverriddenKeys();

    /// <summary>
    /// Try to get a value (if its an overridden property)
    /// </summary>
    /// <param name="property">The property name</param>
    /// <returns>null if we should fallback to base JSON resolution</returns>
    [CanBeNull]
    [MoonSharpHidden]
    public abstract DynValue TryToGet(string property);

    /// <summary>
    /// Try to set a value (if its an overridden property)
    /// </summary>
    /// <param name="property">The property</param>
    /// <param name="value">The value</param>
    /// <returns>false if we should fallback to base JSON resolution</returns>
    [MoonSharpHidden]
    public abstract bool TryToSet(string property, DynValue value);
    
    /// <summary>
    /// Try to delete a value (if its an overridden property0
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>false if we should fallback to base JSON resolution</returns>
    [MoonSharpHidden]
    public abstract bool TryToRemove(string property);

    /// <inheritdoc />
    public override IEnumerable<string> Keys()
    {
        HashSet<string> overriddenKeys = new HashSet<string>();
        foreach (var key in GetExtraAndOverriddenKeys())
        {
            overriddenKeys.Add(key);
            yield return key;
        }

        foreach (var key in base.Keys().Where(key => !overriddenKeys.Contains(key)))
        {
            yield return key;
        }
    }

    /// <inheritdoc />
    public override void Remove(string key)
    {
        if (TryToRemove(key)) return;
        base.Remove(key);
    }

    /// <inheritdoc />
    public override DynValue this[string index]
    {
        get
        {
            if (TryToGet(index) is { } result) return result;
            return base[index];
        }
        set
        {
            if (TryToSet(index, value)) return;
            base[index] = value;
        }
    }
}