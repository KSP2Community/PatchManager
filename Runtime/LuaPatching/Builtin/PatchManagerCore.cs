using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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
    /// Registers a patch that copies each asset it matches under a new name in the same label, then runs
    /// <paramref name="patchMethod" /> on the copy.
    /// </summary>
    /// <remarks>
    /// The duplicate is a patch like any other: it takes a pass, ordering and requirements, and the copy is patched
    /// by everything ordered after it. Internal ID fields are left alone. The patch is named
    /// <c>Duplicate(source -> newName)</c> under the host mod, with the source and new name filled in.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces the patch name.</param>
    /// <param name="converter">The name of the converter to use, as registered via <see cref="Attributes.ConverterAttribute" />.</param>
    /// <param name="label">The addressables label whose assets to copy, which the copies keep.</param>
    /// <param name="source">The name of the asset to copy. Supports <c>*</c> and <c>?</c> wildcards.</param>
    /// <param name="newName">The name of the copy, with <c>{name}</c> standing in for the source asset's name.</param>
    /// <param name="patchMethod">The callback to run on each copy, or <c>null</c> to copy without changes.</param>
    /// <returns>The registered patch, suitable for chaining.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="converter" /> is not registered.</exception>
    public PatchDefinition Duplicate(ScriptExecutionContext context, string converter, string label, string source,
        string newName, [CanBeNull] Func<DynValue, string> patchMethod = null)
    {
        var patch = Patch(context, converter, label, $"Duplicate({source} -> {newName})").Named(source);
        patch.DuplicateNameTemplate = newName;
        if (patchMethod != null)
        {
            patch.Do(patchMethod);
        }

        return patch;
    }

    /// <summary>
    /// Returns the pass with the given name, declaring it when it does not exist yet.
    /// </summary>
    /// <remarks>
    /// A declared pass runs after Early and before Late until <see cref="PassDefinition.ClearOrdering" /> removes
    /// those constraints.
    /// </remarks>
    /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global namespaces <paramref name="name" />.</param>
    /// <param name="name">The pass name, namespaced to the host mod when it is not built-in and does not already carry a namespace.</param>
    /// <returns>The pass, for chaining its ordering.</returns>
    public PassDefinition Pass(ScriptExecutionContext context, string name)
    {
        if (!_universe.RegistrationOpen)
        {
            throw new ScriptRuntimeException($"PM:Pass('{name}') can only be called during patch registration, not from a Do callback or at runtime.");
        }

        return _universe.GetOrAddPass(context.CurrentGlobalEnv.Get("ModId").CastToString(), name);
    }

    /// <summary>
    /// Begins a declarative prefab patch for one stock Addressables key.
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
        if (target.Type != DataType.String)
        {
            throw new ScriptRuntimeException(
                "PM:Prefab expects the stock prefab's Addressables key."
            );
        }
        var identity =
            global::PatchManager.PrefabPatching.PrefabPatchPrefabIdentity
                .FromAddress(target.String);
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
