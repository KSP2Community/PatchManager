using System.Collections.Generic;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using UniLinq;

namespace PatchManager.LuaPatching.Utility;

/// <summary>
/// JSON-object UserData base class that lets subclasses override or add string-keyed properties.
/// </summary>
/// <remarks>
/// Subclasses implement the <see cref="TryToGet" /> / <see cref="TryToSet" /> / <see cref="TryToRemove" /> hooks
/// to intercept accesses by property name, and <see cref="GetExtraAndOverriddenKeys" /> to advertise the names
/// they handle. The base class wires these hooks into the inherited string indexer, <see cref="Remove(string)" />,
/// and <see cref="Keys" />: if a hook handles the access it short-circuits, otherwise the call falls through to
/// the underlying JSON. This is the right base for wrappers that need synthetic properties (computed values, type
/// shims) layered on top of an existing JSON document.
/// </remarks>
public abstract class ExtensibleJsonUserData : JsonUserData
{
    /// <summary>
    /// Creates a new extensible wrapper around the given JSON token.
    /// </summary>
    /// <param name="token">The token to wrap.</param>
    protected ExtensibleJsonUserData(JToken token) : base(token)
    {
    }

    /// <summary>
    /// Returns the property names this subclass handles via the <c>TryToX</c> hooks, whether they shadow existing
    /// JSON keys or add new ones.
    /// </summary>
    /// <remarks>
    /// Used by <see cref="Keys" /> to merge subclass-provided names with the underlying object's keys, with
    /// subclass names taking precedence on collision.
    /// </remarks>
    /// <returns>The property names the subclass handles.</returns>
    [MoonSharpHidden]
    public abstract IEnumerable<string> GetExtraAndOverriddenKeys();

    /// <summary>
    /// Returns the subclass-handled value for the given property, or <c>null</c> to fall through to the underlying JSON.
    /// </summary>
    /// <param name="property">The property name being read.</param>
    /// <returns>The value to expose to Lua, or <c>null</c> when this property is not subclass-handled.</returns>
    [CanBeNull]
    [MoonSharpHidden]
    public abstract DynValue TryToGet(string property);

    /// <summary>
    /// Attempts to handle a write to the given property. Returns whether the subclass consumed the assignment.
    /// </summary>
    /// <param name="property">The property name being written.</param>
    /// <param name="value">The value being assigned.</param>
    /// <returns>True if the subclass handled the assignment, false to fall through to the underlying JSON.</returns>
    [MoonSharpHidden]
    public abstract bool TryToSet(string property, DynValue value);

    /// <summary>
    /// Attempts to handle a delete on the given property. Returns whether the subclass consumed the request.
    /// </summary>
    /// <param name="property">The property name being removed.</param>
    /// <returns>True if the subclass handled the removal, false to fall through to the underlying JSON.</returns>
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
    protected override DynValue TryGetVirtual(DynValue key)
    {
        if (key.Type == DataType.String && TryToGet(key.String) is { } result) return result;
        return null;
    }

    /// <inheritdoc />
    protected override bool TrySetVirtual(DynValue key, DynValue value)
    {
        return key.Type == DataType.String && TryToSet(key.String, value);
    }
}
