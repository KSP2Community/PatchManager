using System;
using MoonSharp.Interpreter;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;

namespace PatchManager.Generic;

/// <summary>
/// Lua submodule exposed as <c>PM.JSON</c>, providing label-untyped patch and asset-creation helpers backed by
/// the generic <c>JSON</c> converter.
/// </summary>
/// <remarks>
/// Use this when patching addressables labels that do not have a domain-specific submodule, or to bypass the
/// typed UserData wrappers and operate directly on raw <see cref="JsonUserData" />.
/// </remarks>
[PatchManagerModule("JSON")]
[MoonSharpUserData]
public class GenericLuaModule
{
    private readonly PatchManagerCore _core;
    private readonly Universe _universe;

    /// <summary>
    /// Creates the submodule bound to the given core and universe.
    /// </summary>
    /// <param name="pmc">The shared <c>PM</c> core instance.</param>
    /// <param name="universe">The owning universe.</param>
    public GenericLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }
    
    /// <summary>
    /// Registers a JSON patch under <paramref name="label" /> with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// The patch matches every asset under <paramref name="label" /> by default; restrict it via <see cref="PatchDefinition.Named" />.
    /// </remarks>
    /// <param name="script">The host Lua script; its <c>ModId</c> global is used to namespace <paramref name="name" />.</param>
    /// <param name="label">The addressables label to patch.</param>
    /// <param name="name">The patch's local name; namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition Patch(Script script, string label, string name)
    {
        return _core.Patch(script, "JSON", label, name);
    }

    /// <summary>
    /// Queues a brand-new JSON asset for creation under the given label and name.
    /// </summary>
    /// <param name="label">The addressables label to tag the asset with.</param>
    /// <param name="name">The asset's addressables address.</param>
    /// <param name="value">The asset's Lua-facing value (typically a <see cref="JsonUserData" /> wrapping a <c>JObject</c> or <c>JArray</c>).</param>
    public void New(string label, string name, DynValue value)
    {
        _core.New("JSON", label, name, value);
    }
}
