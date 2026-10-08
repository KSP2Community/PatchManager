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
    /// The patch matches every asset under <paramref name="label" /> by default. Restrict it via <see cref="PatchDefinition.Named" />.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="label">The addressables label to patch.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition Patch(ScriptExecutionContext context, string label, string name)
    {
        return _core.Patch(context, "JSON", label, name);
    }

    /// <summary>
    /// Registers a patch that copies each asset it matches under a new name in the same label, then runs
    /// <paramref name="patchMethod" /> on the copy.
    /// </summary>
    /// <remarks>
    /// Restrict the pass and ordering like any other patch. Internal ID fields in the copy are left alone.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces the patch name.</param>
    /// <param name="label">The addressables label whose assets to copy, which the copies keep.</param>
    /// <param name="source">The name of the asset to copy. Supports <c>*</c> and <c>?</c> wildcards.</param>
    /// <param name="newName">The name of the copy, with <c>{name}</c> standing in for the source's name.</param>
    /// <param name="patchMethod">The callback to run on each copy, or <c>null</c> to copy without changes.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition Duplicate(ScriptExecutionContext context, string label, string source, string newName,
        Func<DynValue, string> patchMethod = null)
    {
        return _core.Duplicate(context, "JSON", label, source, newName, patchMethod);
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
