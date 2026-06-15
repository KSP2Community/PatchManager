using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching.Builtin;

/// <summary>
/// The PatchManager Lua library, exposed to scripts as the global <c>PM</c>.
/// </summary>
[MoonSharpUserData]
public class PatchManagerCore
{
    private Universe _universe;

    /// <summary>
    /// Creates the library bound to the given universe.
    /// </summary>
    /// <param name="universe">The universe whose submodules and patch registry this library wraps.</param>
    public PatchManagerCore(Universe universe)
    {
        _universe = universe;
    }

    /// <summary>
    /// Returns the registered submodule with the given name (for example <c>PM.Planets</c>).
    /// </summary>
    /// <param name="name">The submodule name as registered via <see cref="Attributes.PatchManagerModuleAttribute" />.</param>
    /// <returns>The submodule's UserData wrapper.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when no submodule with the given name is registered.</exception>
    public DynValue this[string name] =>
        _universe.Submodules.TryGetValue(name, out var submodule)
            ? submodule
            : throw new ScriptRuntimeException($"No PatchManager submodule named '{name}'.");


    #region Core Methods

    /// <summary>
    /// Registers a patch keyed by the given addressables label and namespaced patch name.
    /// </summary>
    /// <param name="script">The host Lua script; its <c>ModId</c> global is used to namespace <paramref name="name" />.</param>
    /// <param name="converter">The name of the converter to use, as registered via <see cref="Attributes.ConverterAttribute" />.</param>
    /// <param name="label">The addressables label whose assets to patch.</param>
    /// <param name="name">The patch's local name; the host mod's ID is prepended to form the full namespaced name.</param>
    /// <returns>The registered patch, suitable for chaining (for example <see cref="PatchDefinition.Do" />).</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="converter" /> is not registered.</exception>
    public PatchDefinition Patch(Script script, string converter, string label, string name)
    {
        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new ScriptRuntimeException($"Unknown converter {converter}");
        }

        var modId = script.Globals.Get("ModId").CastToString();
        var actualName =  modId + ':' + name;

        var newPatch = new PatchDefinition
        {
            ConverterInstance = converterInstance,
            Label = label,
            Name = actualName,
            PatchModId = modId
        };
        _universe.AddPatch(newPatch);
        return newPatch;
    }

    /// <summary>
    /// Queues a brand-new asset for creation under the given label and address.
    /// </summary>
    /// <param name="converter">The name of the converter that will serialize <paramref name="newObject" /> to JSON.</param>
    /// <param name="label">The addressables label to tag the new asset with.</param>
    /// <param name="name">The asset's addressables address (globally unique).</param>
    /// <param name="newObject">The Lua-facing value for the new asset.</param>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="converter" /> is not registered.</exception>
    public void New(string converter, string label, string name, DynValue newObject)
    {
        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new ScriptRuntimeException($"Unknown converter {converter}");
        }

        var newPatch = new LuaAsset()
        {
            ConverterInstance = converterInstance,
            Label = label,
            Name = name,
            CurrentValue = newObject,
        };

        _universe.AddAsset(newPatch);
    }
    #endregion

    #region Other Methods

    /// <summary>
    /// Returns whether the mod with the given ID is loaded.
    /// </summary>
    /// <param name="modId">The mod ID to test.</param>
    /// <returns>True if the mod is loaded, false otherwise.</returns>
    public bool Loaded(string modId)
    {
        return _universe.AllMods.Contains(modId);
    }
    
    #endregion
}
