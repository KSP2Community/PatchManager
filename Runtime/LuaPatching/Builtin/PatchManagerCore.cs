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
    /// <returns>The registered patch, suitable for chaining (for example <see cref="LuaPatch.Do" />).</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="converter" /> is not registered.</exception>
    public LuaPatch Patch(Script script, string converter, string label, string name)
    {
        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new ScriptRuntimeException($"Unknown converter {converter}");
        }

        var modId = script.Globals.Get("ModId").CastToString();
        var actualName =  modId + ':' + name;

        var newPatch = new LuaPatch
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

    // /// <summary>
    // /// Creates a named stage that runs after the most recent implicit stage from the same host mod.
    // /// </summary>
    // /// <remarks>
    // /// Use this when stages within a mod should run in declaration order. The first <c>ImplicitStage</c> in a mod
    // /// runs after the mod's own base stage, and each subsequent one runs after its predecessor. The mod's
    // /// <c>__post</c> stage is also updated to run after the new stage so the mod's post stage stays at the tail
    // /// of the mod's chain. The stage is registered under <c>"modId:name"</c>; references via
    // /// <see cref="LuaPatch.OnStage" />, <see cref="Stage.Before" />, and <see cref="Stage.After" /> use that
    // /// fully-qualified name.
    // /// </remarks>
    // /// <param name="context">The host Lua script; its <c>ModId</c> global is used to scope the implicit chain.</param>
    // /// <param name="name">The stage name (registered as <c>"modId:name"</c>).</param>
    // /// <returns>The created stage, suitable for chaining (for example <see cref="Stage.Before" /> / <see cref="Stage.After" />).</returns>
    // public Stage ImplicitStage(Script context, string name)
    // {
    //     var modId = context.Globals.Get("ModId").CastToString();
    //     var prefixed = $"{modId}:{name}";
    //     var stage = new Stage();
    //     _universe.AddStage(prefixed, stage);
    //     stage.RunsAfter.Add(_universe.LastImplicitWithinMod.GetValueOrDefault(modId, _universe.LastImplicitGlobal));
    //     _universe.LastImplicitWithinMod[modId] = prefixed;
    //     if (_universe.AllStages.TryGetValue($"{modId}:__post", out var postStage))
    //     {
    //         postStage.RunsAfter.Add(prefixed);
    //     }
    //     return stage;
    // }
    //
    // /// <summary>
    // /// Creates a named stage that runs after the last implicit stage of the entire mod load order.
    // /// </summary>
    // /// <remarks>
    // /// The stage is registered under <c>"modId:name"</c>; references via <see cref="LuaPatch.OnStage" />,
    // /// <see cref="Stage.Before" />, and <see cref="Stage.After" /> use that fully-qualified name.
    // /// </remarks>
    // /// <param name="context">The host Lua script; its <c>ModId</c> global is used to namespace the stage.</param>
    // /// <param name="name">The stage name (registered as <c>"modId:name"</c>).</param>
    // /// <returns>The created stage, suitable for chaining (for example <see cref="Stage.Before" /> / <see cref="Stage.After" />).</returns>
    // public Stage GlobalStage(Script context, string name)
    // {
    //     var modId = context.Globals.Get("ModId").CastToString();
    //     var prefixed = $"{modId}:{name}";
    //     var stage = new Stage();
    //     _universe.AddStage(prefixed, stage);
    //     stage.RunsAfter.Add(_universe.LastImplicitGlobal);
    //     _universe.LastImplicitGlobal = prefixed;
    //     return stage;
    // }
    //
    // /// <summary>
    // /// Creates a named stage with no implicit ordering; callers must declare any required relations explicitly.
    // /// </summary>
    // /// <remarks>
    // /// The stage is registered under <c>"modId:name"</c>; references via <see cref="LuaPatch.OnStage" />,
    // /// <see cref="Stage.Before" />, and <see cref="Stage.After" /> use that fully-qualified name.
    // /// </remarks>
    // /// <param name="context">The host Lua script; its <c>ModId</c> global is used to namespace the stage.</param>
    // /// <param name="name">The stage name (registered as <c>"modId:name"</c>).</param>
    // /// <returns>The created stage, suitable for chaining (for example <see cref="Stage.Before" /> / <see cref="Stage.After" />).</returns>
    // public Stage Stage(Script context, string name)
    // {
    //     var modId = context.Globals.Get("ModId").CastToString();
    //     var prefixed = $"{modId}:{name}";
    //     var stage = new Stage();
    //     _universe.AddStage(prefixed, stage);
    //     return stage;
    // }

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
