using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp2.API.Features;
using SpaceWarp2.API.Loading;

namespace PatchManager.LuaPatching.Builtin
{
    /// <summary>
    /// The PatchManager Lua library, exposed to scripts as the global <c>PM</c>.
    /// </summary>
    [MoonSharpUserData]
    public sealed class PatchManagerCore
    {
        private readonly Universe _universe;

        // Features added by the PM:WithFeature scopes currently open, outermost first.
        private readonly List<string> _scopedFeatures = new();

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
        /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global is used to namespace <paramref name="name" />.</param>
        /// <param name="converter">The name of the converter to use, as registered via <see cref="Attributes.ConverterAttribute" />.</param>
        /// <param name="label">The addressables label whose assets to patch.</param>
        /// <param name="name">The patch's local name. The host mod's ID is prepended to form the full namespaced name.</param>
        /// <returns>The registered patch, suitable for chaining (for example <see cref="PatchDefinition.Do" />).</returns>
        /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="converter" /> is not registered.</exception>
        public PatchDefinition Patch(ScriptExecutionContext context, string converter, string label, string name)
        {
            if (!_universe.RegistrationOpen)
                throw new ScriptRuntimeException($"PM:Patch('{name}') can only be called during patch registration, not from a Do callback or at runtime.");

            if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
                throw new ScriptRuntimeException($"Unknown converter {converter}");

            var modId = context.CurrentGlobalEnv.Get("ModId").CastToString();
            var actualName = modId + ':' + name;

            var newPatch = new PatchDefinition
            {
                ConverterInstance = converterInstance,
                Label = label,
                Name = actualName,
                PatchModId = modId
            };
            newPatch.NeedsFeatures.UnionWith(_scopedFeatures);
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
                throw new ScriptRuntimeException($"PM:Pass('{name}') can only be called during patch registration, not from a Do callback or at runtime.");

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
            var builder = new global::PatchManager.PrefabPatching.PrefabPatchBuilder(
                modId,
                name,
                identity
            );
            builder.NeedsFeature(_scopedFeatures.ToArray());
            return new PrefabPatchLuaBuilder(builder);
        }

        /// <summary>
        /// Queues a brand-new asset for creation under the given label and address.
        /// </summary>
        /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global names the creating mod.</param>
        /// <param name="converter">The name of the converter that will serialize <paramref name="newObject" /> to JSON.</param>
        /// <param name="label">The addressables label to tag the new asset with.</param>
        /// <param name="name">The asset's addressables address (globally unique).</param>
        /// <param name="newObject">The Lua-facing value for the new asset.</param>
        /// <returns>The queued asset, for gating it with features.</returns>
        /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="converter" /> is not registered.</exception>
        public LuaAsset New(ScriptExecutionContext context, string converter, string label, string name, DynValue newObject)
        {
            var asset = New(converter, label, name, newObject);
            asset.ModId = ModIdOf(context);
            asset.NeedsFeatures.UnionWith(_scopedFeatures);
            return asset;
        }

        /// <summary>
        /// Declares a feature of the calling mod.
        /// </summary>
        /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global names the declaring mod.</param>
        /// <param name="id">The feature ID, namespaced to the mod when it does not already carry a namespace.</param>
        /// <param name="options">An optional table with <c>requires</c> and <c>conflicts</c> lists of feature IDs.</param>
        /// <exception cref="ScriptRuntimeException">Thrown when called after patch registration has closed.</exception>
        public void DefineFeature(ScriptExecutionContext context, string id, Table options = null)
        {
            if (!_universe.RegistrationOpen)
                throw new ScriptRuntimeException($"PM:DefineFeature('{id}') can only be called during patch registration.");

            _universe.FeatureRegistry.Define(
                ModIdOf(context),
                id,
                StringsIn(options?.Get("requires")),
                StringsIn(options?.Get("conflicts"))
            );
        }

        /// <summary>
        /// Runs <paramref name="body" /> with a feature added to every patch, created asset, prefab patch and run
        /// callback it registers.
        /// </summary>
        /// <remarks>
        /// Scopes nest, so a registration inside two scopes needs both features. A file whose patches all belong to one
        /// feature wraps its body once instead of chaining <c>:Feature</c> onto every registration.
        /// </remarks>
        /// <param name="context">The Lua execution context.</param>
        /// <param name="id">The feature ID, namespaced to the calling mod when it does not already carry a namespace.</param>
        /// <param name="body">The function whose registrations get the feature.</param>
        public void WithFeature(ScriptExecutionContext context, string id, Closure body)
        {
            _scopedFeatures.Add(FeatureRegistry.Qualify(ModIdOf(context), id));
            try
            {
                body.Call();
            }
            finally
            {
                _scopedFeatures.RemoveAt(_scopedFeatures.Count - 1);
            }
        }

        /// <summary>
        /// Registers a callback that runs before each feature set's patch run.
        /// </summary>
        /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global names the registering mod.</param>
        /// <param name="callback">The callback, handed the set being built as a table of each id mapped to true.</param>
        /// <returns>The registered callback, for gating it with features.</returns>
        public FeatureGatedAction OnBeginRun(ScriptExecutionContext context, Closure callback) =>
            AddRunCallback(_universe.BeginRunCallbacks, context, callback);

        /// <summary>
        /// Registers a callback that runs after each feature set's patch run.
        /// </summary>
        /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global names the registering mod.</param>
        /// <param name="callback">The callback, handed the set that was built as a table of each id mapped to true.</param>
        /// <returns>The registered callback, for gating it with features.</returns>
        public FeatureGatedAction OnEndRun(ScriptExecutionContext context, Closure callback) =>
            AddRunCallback(_universe.EndRunCallbacks, context, callback);

        /// <summary>
        /// Registers a callback that runs whenever a feature set becomes active, for data a mod keeps per set.
        /// </summary>
        /// <remarks>
        /// Runs at boot for the main menu set and on every switch to another set, never on a reload into the
        /// same set, and only while the mod and the callback's selected features are on.
        /// </remarks>
        /// <param name="context">The Lua execution context. Its env's <c>ModId</c> global names the registering mod.</param>
        /// <param name="callback">The callback, handed the set just activated as a table of each id mapped to true.</param>
        /// <returns>The registered action, for gating it with features.</returns>
        public FeatureGatedAction OnSetActivated(ScriptExecutionContext context, Closure callback)
        {
            string modId = ModIdOf(context);
            Script script = context.GetScript();
            FeatureGatedAction action = Loading.AddSetAction(
                modId,
                $"{modId} set data",
                () => { },
                done =>
                {
                    // One mod's failing callback must not stop the others from loading their data
                    try
                    {
                        callback.Call(SetTable(script, ActiveFeatures.Enabled ?? Array.Empty<string>()));
                    }
                    catch (InterpreterException e)
                    {
                        Shared.Logging.LogError($"{modId}'s set activation callback failed: {e.DecoratedMessage}");
                    }

                    done();
                }
            );
            foreach (string feature in _scopedFeatures)
            {
                action.Feature(feature);
            }

            return action;
        }

        private FeatureGatedAction AddRunCallback(
            List<FeatureGatedAction> into,
            ScriptExecutionContext context,
            Closure callback
        )
        {
            if (!_universe.RegistrationOpen)
                throw new ScriptRuntimeException("Run callbacks can only be registered during patch registration.");

            string modId = ModIdOf(context);
            Script script = context.GetScript();
            var registered = new FeatureGatedAction(modId, $"{modId} run callback", done =>
            {
                callback.Call(SetTable(script, _universe.EnabledFeatures ?? new HashSet<string>()));
                done();
            });
            foreach (string feature in _scopedFeatures)
            {
                registered.Feature(feature);
            }

            into.Add(registered);
            return registered;
        }

        // A set as Lua sees it, which is how the Features global hands it out too
        private static DynValue SetTable(Script script, IEnumerable<string> ids)
        {
            var set = new Table(script);
            foreach (string id in ids)
            {
                set[id] = true;
            }

            return DynValue.NewTable(set);
        }

        private static string ModIdOf(ScriptExecutionContext context) =>
            context.CurrentGlobalEnv.Get("ModId").CastToString();

        private static IEnumerable<string> StringsIn(DynValue list)
        {
            if (list == null || list.Type != DataType.Table)
                yield break;

            foreach (var value in list.Table.Values)
            {
                if (value.Type == DataType.String)
                    yield return value.String;
            }
        }

        /// <summary>
        /// Queues a brand-new asset for creation under the given label and address, without an owning mod.
        /// </summary>
        /// <param name="converter">The name of the converter that will serialize <paramref name="newObject" /> to JSON.</param>
        /// <param name="label">The addressables label to tag the new asset with.</param>
        /// <param name="name">The asset's addressables address (globally unique).</param>
        /// <param name="newObject">The Lua-facing value for the new asset.</param>
        /// <returns>The queued asset.</returns>
        /// <exception cref="ScriptRuntimeException">Thrown when <paramref name="converter" /> is not registered.</exception>
        [MoonSharpHidden]
        public LuaAsset New(string converter, string label, string name, DynValue newObject)
        {
            if (!_universe.RegistrationOpen)
                throw new ScriptRuntimeException($"PM:New('{name}') can only be called during patch registration, not from a Do callback or at runtime.");

            if (!Universe.Converters.TryGetValue(converter, out var converterInstance))
                throw new ScriptRuntimeException($"Unknown converter {converter}");

            var newPatch = new LuaAsset
            {
                ConverterInstance = converterInstance,
                Label = label,
                Name = name,
                CurrentValue = newObject,
            };

            _universe.AddAsset(newPatch);
            return newPatch;
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
}
