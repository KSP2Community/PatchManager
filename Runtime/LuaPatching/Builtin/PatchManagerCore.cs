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
    /// <exception cref="KeyNotFoundException">Thrown when no submodule with the given name is registered.</exception>
    public DynValue this[string name] => _universe.Submodules[name];


    #region Core Methods
    /// <summary>
    /// Registers a patch that runs against every asset under the given addressables label.
    /// </summary>
    /// <param name="context">The host Lua script; its <c>ModId</c> global is used as the patch's default stage.</param>
    /// <param name="converter">The name of the converter to use, as registered via <see cref="Attributes.ConverterAttribute" />.</param>
    /// <param name="label">The addressables label whose assets to patch.</param>
    /// <param name="method">The patch callback. Returns <c>"remove"</c> to delete the asset, <c>null</c> to keep it.</param>
    /// <returns>The registered patch, suitable for chaining (for example <see cref="LuaPatch.OnStage" />).</returns>
    /// <exception cref="Exception">Thrown when <paramref name="converter" /> is not registered, or when <paramref name="method" /> is null.</exception>
    public LuaPatch PatchAll(Script context, string converter, string label, Func<DynValue, string> method)
    {
        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new Exception($"Unknown converter {converter}");
        }

        if (method == null)
        {
            throw new Exception($"Registering {converter}:{label} with null method");
        }

        var newPatch = new LuaPatch
        {
            ConverterInstance = converterInstance,
            Label = label,
            Name = null,
            PatchMethod = method,
            Stage = context.Globals.Get("ModId").CastToString()
        };
        _universe.AddPatch(newPatch);
        return newPatch;
    }

    /// <summary>
    /// Registers a patch that runs against assets under the given label whose name matches <paramref name="name" />.
    /// </summary>
    /// <remarks>
    /// <paramref name="name" /> may include <c>*</c> and <c>?</c> wildcards, expanded by <see cref="Universe.MatchesPattern" />.
    /// </remarks>
    /// <param name="context">The host Lua script; its <c>ModId</c> global is used as the patch's default stage.</param>
    /// <param name="converter">The name of the converter to use, as registered via <see cref="Attributes.ConverterAttribute" />.</param>
    /// <param name="label">The addressables label to patch.</param>
    /// <param name="name">The addressables address pattern to match.</param>
    /// <param name="method">The patch callback. Returns <c>"remove"</c> to delete the asset, <c>null</c> to keep it.</param>
    /// <returns>The registered patch, suitable for chaining (for example <see cref="LuaPatch.OnStage" />).</returns>
    /// <exception cref="Exception">Thrown when <paramref name="converter" /> is not registered, or when <paramref name="method" /> is null.</exception>
    public LuaPatch Patch(Script context, string converter, string label, string name, Func<DynValue, string> method)
    {
        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new Exception($"Unknown converter {converter}");
        }
        if (method == null)
        {
            throw new Exception($"Registering {converter}:{label}:{name} with null method");
        }

        var newPatch = new LuaPatch
        {
            ConverterInstance = converterInstance,
            Label = label,
            Name = name,
            PatchMethod = method,
            Stage = context.Globals.Get("ModId").CastToString()
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
    /// <exception cref="Exception">Thrown when <paramref name="converter" /> is not registered.</exception>
    public void New(string converter, string label, string name, DynValue newObject)
    {
        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new Exception($"Unknown converter {converter}");
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

    /// <summary>
    /// Creates a named stage that runs after the most recent implicit stage from the same host mod.
    /// </summary>
    /// <remarks>
    /// Use this when stages within a mod should run in declaration order. The first <c>ImplicitStage</c> in a mod
    /// runs after the mod's own setup stage, and each subsequent one runs after its predecessor.
    /// </remarks>
    /// <param name="context">The host Lua script; its <c>ModId</c> global is used to scope the implicit chain.</param>
    /// <param name="name">The stage name.</param>
    /// <returns>The created stage, suitable for chaining (for example <see cref="Stage.Before" /> / <see cref="Stage.After" />).</returns>
    public Stage ImplicitStage(Script context, string name)
    {
        var stage = new Stage();
        _universe.AddStage(name, stage);
        stage.RunsAfter.Add(_universe.LastImplicitWithinMod.GetValueOrDefault(
            context.Globals.Get("ModId").CastToString(), _universe.LastImplicitGlobal));
        return stage;
    }

    /// <summary>
    /// Creates a named stage that runs after the last implicit stage of the entire mod load order.
    /// </summary>
    /// <param name="name">The stage name.</param>
    /// <returns>The created stage, suitable for chaining (for example <see cref="Stage.Before" /> / <see cref="Stage.After" />).</returns>
    public Stage GlobalStage(string name)
    {
        var stage = new Stage();
        _universe.AddStage(name, stage);
        stage.RunsAfter.Add(_universe.LastImplicitGlobal);
        return stage;
    }

    /// <summary>
    /// Creates a named stage with no implicit ordering; callers must declare any required relations explicitly.
    /// </summary>
    /// <param name="name">The stage name.</param>
    /// <returns>The created stage, suitable for chaining (for example <see cref="Stage.Before" /> / <see cref="Stage.After" />).</returns>
    public Stage Stage(string name)
    {
        var stage = new Stage();
        _universe.AddStage(name, stage);
        return stage;
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
