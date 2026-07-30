using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching.Builtin;

/// <summary>
/// The PatchManager Lua library, exposed to scripts as the global <c>PM</c>.
/// </summary>
[MoonSharpUserData]
public sealed class PatchManagerCore
{
    private readonly Universe _universe;

    /// <summary>
    /// Creates the library bound to the given universe.
    /// </summary>
    /// <param name="universe">The universe whose submodules and patch registry this library wraps.</param>
    public PatchManagerCore(Universe universe)
    {
        _universe = universe;
    }

    /// <summary>
    /// The metadata tag a config value carries to invalidate the patch cache when its value changes between
    /// launches.
    /// </summary>
    /// <remarks>
    /// The canonical constant. PM's own consumer and any C# config reference this.
    /// </remarks>
    public const string InvalidatesOnChangeTag = "InvalidatesPatchManagerOnChange";

    /// <summary>
    /// The cache-invalidation tag, exposed to scripts as <c>PM.InvalidatesOnChange</c> so a config entry can
    /// be tagged with <c>:Tag(PM.InvalidatesOnChange)</c>.
    /// </summary>
    public string InvalidatesOnChange => InvalidatesOnChangeTag;

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
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global is used to namespace <paramref name="name" />.</param>
    /// <param name="converter">The name of the converter to use, as registered via <see cref="Attributes.ConverterAttribute" />.</param>
    /// <param name="label">The addressables label whose assets to patch.</param>
    /// <param name="name">The patch's local name. The host mod's ID is prepended to form the full namespaced name.</param>
    /// <returns>The registered patch, suitable for chaining (for example <see cref="PatchDefinition.Do" />).</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="converter" /> is not registered.</exception>
    public PatchDefinition Patch(ScriptExecutionContext context, string converter, string label, string name)
    {
        if (!_universe.RegistrationOpen)
        {
            throw new ScriptRuntimeException($"PM:Patch('{name}') can only be called during patch registration, not from a Do callback or at runtime.");
        }

        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new ScriptRuntimeException($"Unknown converter {converter}");
        }

        var modId = context.CurrentGlobalEnv.Get("ModId").CastToString();
        var actualName = modId + ':' + name;

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
    /// Begins a declarative prefab patch. Normal Lua authoring passes an
    /// Addressables key string; generated tooling may pass a complete
    /// PrefabPatchPrefabIdentity table.
    /// </summary>
    public PrefabPatchLuaBuilder Prefab(
        ScriptExecutionContext context,
        string name,
        DynValue target
    )
    {
        if (!_universe.RegistrationOpen)
        {
            throw new ScriptRuntimeException(
                $"PM:Prefab('{name}') can only be called during patch "
                    + "registration."
            );
        }

        var modId = context.CurrentGlobalEnv
            .Get("ModId")
            .CastToString();
        var identity = target.Type == DataType.String
            ? global::PatchManager.PrefabPatching.PrefabPatchPrefabIdentity
                .FromAddress(target.String)
            : PrefabPatchLuaBuilder.Model<
                global::PatchManager.PrefabPatching.PrefabPatchPrefabIdentity
            >(target, "prefab identity or Addressables key");
        return new PrefabPatchLuaBuilder(
            new global::PatchManager.PrefabPatching.PrefabPatchBuilder(
                modId,
                name,
                identity
            )
        );
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
        if (!_universe.RegistrationOpen)
        {
            throw new ScriptRuntimeException($"PM:New('{name}') can only be called during patch registration, not from a Do callback or at runtime.");
        }

        if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
        {
            throw new ScriptRuntimeException($"Unknown converter {converter}");
        }

        var newPatch = new LuaAsset
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
