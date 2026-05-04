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
    /// Registers a patch that runs against every part definition.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the part, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch PatchAll(Script script, Func<PartUserData, string> callback)
    {
        return _core.PatchAll(script, "Part", "parts_data", callback.ToPatchMethod());
    }

    /// <summary>
    /// Registers a patch that runs against the part matching <paramref name="name" />.
    /// </summary>
    /// <param name="script">The host Lua script.</param>
    /// <param name="name">The part name pattern (supports <c>*</c> and <c>?</c> wildcards).</param>
    /// <param name="callback">The patch callback. Returns <c>"remove"</c> to delete the part, <c>null</c> to keep it.</param>
    /// <returns>The registered patch.</returns>
    public LuaPatch Patch(Script script, string name, Func<PartUserData, string> callback)
    {
        return _core.Patch(script, "Part", "parts_data", name, callback.ToPatchMethod());
    }
}
