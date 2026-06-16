using System;
using MoonSharp.Interpreter;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.LuaPatching.Utility;
using PatchManager.Parts.UserData;

namespace PatchManager.Parts;

/// <summary>
/// Lua submodule exposed as <c>PM.Parts</c>, providing patches for part definitions.
/// </summary>
[PatchManagerModule("Parts")]
[MoonSharpUserData]
public class PartsLuaModule
{
    private PatchManagerCore _core;
    private Universe _universe;

    /// <summary>
    /// Creates the submodule bound to the given core and universe.
    /// </summary>
    /// <param name="pmc">The shared <c>PM</c> core instance.</param>
    /// <param name="universe">The owning universe.</param>
    public PartsLuaModule(PatchManagerCore pmc, Universe universe)
    {
        _core = pmc;
        _universe = universe;
    }

    /// <summary>
    /// Registers a part patch with the given namespaced patch name.
    /// </summary>
    /// <remarks>
    /// The patch matches every part by default. Restrict it via <see cref="PatchDefinition.Named" />, which supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The patch's local name, namespaced with the host mod's ID.</param>
    /// <returns>The registered patch.</returns>
    public PatchDefinition Patch(ScriptExecutionContext context, string name)
    {
        return _core.Patch(context, "Part", "parts_data", name);
    }
}
