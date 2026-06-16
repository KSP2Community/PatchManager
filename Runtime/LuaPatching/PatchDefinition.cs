using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching.Utility;
using PatchManager.Shared;

namespace PatchManager.LuaPatching;

/// <summary>
/// A registered patch operation: which converter, which addressables target, what callback to run, and at which stage.
/// </summary>
[MoonSharpUserData]
public class PatchDefinition
{
    /// <summary>
    /// The pass a patch runs in.
    /// </summary>
    /// <remarks>
    /// Passes are full sweeps over every patched label: the <see cref="Early" /> pass runs across
    /// every label first, then <see cref="Default" />, then <see cref="Late" />. JToken state is held
    /// in memory between passes so later passes see the output of earlier ones.
    /// </remarks>
    public enum PatchPass {
        /// <summary>
        /// Runs first; typically reserved for reading created or existing assets into shared state.
        /// </summary>
        Early,
        /// <summary>
        /// Runs second; the default pass where most patches apply their changes.
        /// </summary>
        Default,
        /// <summary>
        /// Runs third; typically reserved for final writeback from shared state.
        /// </summary>
        Late
    }

    /// <summary>
    /// Ordering bucket within a single pass.
    /// </summary>
    /// <remarks>
    /// Buckets sort independently. A patch's <c>:Before</c> / <c>:After</c> targets in a different
    /// bucket are silently ignored, like references to nonexistent patches. <c>:Needs</c> and
    /// <c>:Conflicts</c> remain global across passes and buckets.
    /// </remarks>
    public enum PatchOrdering
    {
        /// <summary>
        /// Runs before every Default and Last patch in the same pass.
        /// </summary>
        First,
        /// <summary>
        /// Runs after every First patch and before every Last patch in the same pass.
        /// </summary>
        Default,
        /// <summary>
        /// Runs after every First and Default patch in the same pass.
        /// </summary>
        Last
    }
    
    /// <summary>
    /// The converter that produces the Lua value the patch operates on and serializes the result back to JSON.
    /// </summary>
    [MoonSharpHidden] public IConverter ConverterInstance;

    /// <summary>
    /// The user-supplied callback that runs against each matched asset.
    /// </summary>
    /// <remarks>
    /// Returns <c>"remove"</c> (case-insensitive) to delete the asset, or <c>null</c> to keep it. The callback
    /// mutates the wrapped JSON in place; its non-removal return value is otherwise unused.
    /// </remarks>
    [MoonSharpHidden] [CanBeNull] public Func<DynValue, string> PatchMethod;

    /// <summary>
    /// Sets the patch's apply callback.
    /// </summary>
    /// <remarks>
    /// The callback returns <c>"remove"</c> (case-insensitive) to delete the asset, or <c>null</c> to keep it. The callback
    /// mutates the wrapped JSON in place; its non-removal return value is otherwise unused.
    /// </remarks>
    /// <param name="patchMethod">The supplied callback.</param>
    /// <returns>The patch instance for chaining.</returns>
    /// <exception cref="ScriptRuntimeException">Thrown when a patch method has already been set.</exception>
    public PatchDefinition Do(Func<DynValue, string> patchMethod)
    {
        if (PatchMethod != null)
        {
            throw new ScriptRuntimeException("You can only have one `Do` block per patch");
        }

        PatchMethod = patchMethod;

        return this;
    }
    
    /// <summary>
    /// The addressables label whose assets this patch targets.
    /// </summary>
    public string Label;

    /// <summary>
    /// The patch's namespaced name, typically <c>modId:&lt;supplied-name&gt;</c>.
    /// </summary>
    public string Name;

    /// <summary>
    /// The host mod's ID, used as the namespace for dependency-resolution lookups.
    /// </summary>
    public string PatchModId;

    /// <summary>
    /// The asset names this patch targets, or empty to match every asset under <see cref="Label" />.
    /// </summary>
    /// <remarks>
    /// Supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    public HashSet<string> Names = new();

    /// <summary>
    /// Makes the patch target the assets with these names.
    /// </summary>
    /// <param name="names">The asset names to target.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition Named(params string[] names)
    {
        foreach (var name in names)
        {
            Names.Add(name);
        }
        return this;
    }

    /// <summary>
    /// The asset names this patch rejects.
    /// </summary>
    /// <remarks>
    /// Supports <c>*</c> and <c>?</c> wildcards.
    /// </remarks>
    [MoonSharpHidden] public HashSet<NamePattern> DisallowedNames = new();

    /// <summary>
    /// Makes the patch reject the assets with these names.
    /// </summary>
    /// <param name="names">The asset names to reject.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition NotNamed(params string[] names)
    {
        foreach (var name in names)
        {
            DisallowedNames.Add(NamePattern.Get(name));
        }

        return this;
    }
    
    /// <summary>
    /// The mod GUIDs this patch requires to run.
    /// </summary>
    public HashSet<string> NeedsMods = new();

    /// <summary>
    /// Makes the patch require these mod IDs to run.
    /// </summary>
    /// <param name="ids">The required mod IDs.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition Needs(params string[] ids)
    {
        foreach (var id in ids)
        {
            NeedsMods.Add(id);
        }
        return this;
    }

    /// <summary>
    /// The mod GUIDs this patch refuses to run alongside.
    /// </summary>
    public HashSet<string> ConflictsMods = new();

    /// <summary>
    /// Makes the patch refuse to run alongside these mod IDs.
    /// </summary>
    /// <param name="ids">The conflicting mod IDs.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition Conflicts(params string[] ids)
    {
        foreach (var id in ids)
        {
            ConflictsMods.Add(id);
        }
        return this;
    }

    /// <summary>
    /// The patch IDs that must also run for this patch to run.
    /// </summary>
    public HashSet<string> NeedsPatches = new();

    /// <summary>
    /// Makes the patch require these other patches to run.
    /// </summary>
    /// <remarks>
    /// This set is resolved against the patches already filtered through mod-level requirements.
    /// </remarks>
    /// <param name="ids">The required patch IDs; namespaced to the host mod when they do not already carry a namespace.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition NeedsPatch(params string[] ids)
    {
        foreach (var id in ids)
        {
            NeedsPatches.Add(NormalizeId(id));
        }
        return this;
    }

    /// <summary>
    /// The patch IDs this patch refuses to run alongside.
    /// </summary>
    public HashSet<string> ConflictsPatches = new();

    /// <summary>
    /// Makes the patch refuse to run alongside these patches.
    /// </summary>
    /// <param name="ids">The conflicting patch IDs; namespaced to the host mod when they do not already carry a namespace.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition ConflictsPatch(params string[] ids)
    {
        foreach (var id in ids)
        {
            ConflictsPatches.Add(NormalizeId(id));
        }
        return this;
    }
    
    /// <summary>
    /// The patch IDs this patch runs after when they are present.
    /// </summary>
    public HashSet<string> AfterPatches = new();

    /// <summary>
    /// Makes this patch run after the given patches when they exist.
    /// </summary>
    /// <param name="ids">The patch IDs to run after; namespaced to the host mod when they do not already carry a namespace.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition AfterPatch(params string[] ids)
    {
        foreach (var id in ids)
        {
            AfterPatches.Add(NormalizeId(id));
        }
        return this;
    }

    /// <summary>
    /// The mod IDs whose patches this patch runs after when present.
    /// </summary>
    public HashSet<string> AfterMods = new();

    /// <summary>
    /// Makes this patch run after every patch from the given mods.
    /// </summary>
    /// <param name="ids">The mod IDs whose patches this patch should run after.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition After(params string[] ids)
    {
        foreach (var id in ids)
        {
            AfterMods.Add(id);
        }
        return this;
    }

    /// <summary>
    /// The patch IDs this patch runs before when they are present.
    /// </summary>
    public HashSet<string> BeforePatches = new();

    /// <summary>
    /// Makes this patch run before the given patches when they exist.
    /// </summary>
    /// <param name="ids">The patch IDs to run before; namespaced to the host mod when they do not already carry a namespace.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition BeforePatch(params string[] ids)
    {
        foreach (var id in ids)
        {
            BeforePatches.Add(NormalizeId(id));
        };
        return this;
    }
    
    /// <summary>
    /// The mod IDs whose patches this patch runs before when present.
    /// </summary>
    public HashSet<string> BeforeMods = new();

    /// <summary>
    /// Makes this patch run before every patch from the given mods.
    /// </summary>
    /// <param name="ids">The mod IDs whose patches this patch should run before.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition Before(params string[] ids)
    {
        foreach (var id in ids)
        {
            BeforeMods.Add(id);
        }
        return this;
    }

    private class Predicate
    {
        [CanBeNull] public string Key;
        [CanBeNull] public Func<DynValue, bool> Method;
        [CanBeNull] public string AssertionMessage;
        public bool Invert;
        
        // Returns false if the predicate fails
        public bool Evaluate(string name, DynValue value, Summary summary)
        {
            if (Key == null)
            {
                var result = Method!.Invoke(value);
                if (!result)
                {
                    summary.Skip(name, AssertionMessage ?? "asset failed Requires(...) predicate");
                }

                return result;
            }

            var index = Index(value);
            if (index.Type == DataType.Nil)
            {
                if (Invert) return true;
                summary.Skip(name, $"asset did not have {Key.ToLiteral()}");
                return false;
            }

            if (Invert)
            {
                summary.Skip(name, AssertionMessage ?? $"asset had {Key.ToLiteral()}");
                return false;
            }

            if (Method == null) return true;
            
            var result2 = Method!.Invoke(index);
            if (!result2)
            {
                summary.Skip(name, AssertionMessage ?? $"asset had {Key.ToLiteral()} but the predicate failed");
            }
            return result2;

        }

        private static readonly Script IndexingScript = new(CoreModules.Preset_HardSandbox);

        private DynValue Index(DynValue value)
        {
            switch (value.Type)
            {
                case DataType.Table:
                    return value.Table.Get(Key);
                case DataType.UserData:
                    return value.UserData.Descriptor.Index(IndexingScript, value.UserData.Object,
                        DynValue.NewString(Key), false) ?? DynValue.Nil;
                default:
                    return DynValue.Nil;
            }
        }
    }

    [MoonSharpHidden] private List<Predicate> _predicates = new();


    /// <summary>
    /// Adds a predicate that gates the patch and reports skips through the summary.
    /// </summary>
    /// <remarks>
    /// Preferred over an inline <c>if</c> in the patch body so skipped assets are recorded in the summary.
    /// </remarks>
    /// <param name="predicate">The predicate evaluated against each candidate asset.</param>
    /// <param name="message">Optional message logged when the predicate rejects an asset.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition Requires(Func<DynValue, bool> predicate, [CanBeNull] string message = null)
    {
        _predicates.Add(new Predicate
        {
            Key = null,
            Method = predicate,
            AssertionMessage = message
        });
        return this;
    }

    /// <summary>
    /// Adds a constant gate that disables the patch entirely when <paramref name="gate" /> is
    /// <c>false</c>. Each candidate asset is reported as skipped through the summary.
    /// </summary>
    /// <remarks>
    /// Preferred over a top-level <c>if</c> guard around <c>PM.Patch(...)</c> when the gate value comes
    /// from a config value: registering the patch unconditionally lets the summary report what would
    /// have applied, and the gating config value still binds (and shows in the settings menu) regardless
    /// of the gate's current value.
    /// </remarks>
    /// <param name="gate">The constant value the predicate evaluates to.</param>
    /// <param name="message">Optional message logged when the gate is <c>false</c>.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition Requires(bool gate, [CanBeNull] string message = null)
    {
        _predicates.Add(new Predicate
        {
            Key = null,
            Method = _ => gate,
            AssertionMessage = message
        });
        return this;
    }

    /// <summary>
    /// Adds a requirement that the asset expose <paramref name="key" />, optionally with a predicate against the resolved value.
    /// </summary>
    /// <param name="key">The key the asset must expose.</param>
    /// <param name="predicate">Optional predicate evaluated against the value resolved at <paramref name="key" />, not the asset itself.</param>
    /// <param name="message">Optional assertion message logged when the key is present but the predicate fails.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition Has(string key, [CanBeNull] Func<DynValue, bool> predicate = null,
        [CanBeNull] string message = null)
    {
        _predicates.Add(new Predicate
        {
            Key = key,
            Method = predicate,
            AssertionMessage = message
        });
        return this;
    }
    /// <summary>
    /// Adds a requirement that the asset does not expose <paramref name="key" />
    /// </summary>
    /// <param name="key">The key the asset must not expose.</param>
    /// <param name="message">Optional assertion message logged when the key is present.</param>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition HasNo(string key, [CanBeNull] string message = null)
    {
        _predicates.Add(new Predicate
            {
                Key = key,
                Invert = true,
                AssertionMessage = message
            }
        );
        return this;
    }

    /// <summary>
    /// The ordering bucket the patch belongs to within its pass.
    /// </summary>
    [MoonSharpHidden] public PatchOrdering Ordering = PatchOrdering.Default;

    /// <summary>
    /// Makes the patch run before every Default and Last patch in the same pass.
    /// </summary>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition First()
    {
        Ordering = PatchOrdering.First;
        return this;
    }

    /// <summary>
    /// Makes the patch run after every First and Default patch in the same pass.
    /// </summary>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition Last()
    {
        Ordering = PatchOrdering.Last;
        return this;
    }

    /// <summary>
    /// The pass the patch runs in.
    /// </summary>
    [MoonSharpHidden] public PatchPass Pass = PatchPass.Default;

    /// <summary>
    /// Makes the patch run in the Early pass.
    /// </summary>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition Early()
    {
        Pass = PatchPass.Early;
        return this;
    }

    /// <summary>
    /// Makes the patch run in the Late pass.
    /// </summary>
    /// <returns>The patch instance for chaining.</returns>
    public PatchDefinition Late()
    {
        Pass = PatchPass.Late;
        return this;
    }
    
    /// <summary>
    /// The patch's precomputed run-order index across all registered patches.
    /// </summary>
    [MoonSharpHidden] public int Order;

    /// <summary>
    /// Applies the patch to <paramref name="value" />, recording the outcome in <paramref name="summary" />.
    /// </summary>
    /// <param name="value">The value to patch.</param>
    /// <param name="summary">The summary to record application, skip, or error events into.</param>
    /// <param name="removed">Set to <c>true</c> when the callback signals deletion by returning <c>"remove"</c>.</param>
    /// <param name="errored">Set to <c>true</c> when the callback or a predicate threw, or when no <c>:Do(...)</c> block was registered.</param>
    /// <returns><c>true</c> when the patch ran without error or predicate failure, <c>false</c> otherwise.</returns>
    public bool Apply(DynValue value, Summary summary, out bool removed, out bool errored)
    {
        removed = false;
        errored = false;
        if (value.Type == DataType.Nil)
        {
            summary.Skip(Name, "asset is nil");
            return false;
        }

        if (PatchMethod == null)
        {
            errored = true;
            summary.Error(Name, "no :Do(...) block exists for this patch");
            return false;
        }

        try
        {
            // Not converting to LINQ as this is a hot path
            foreach (var predicate in _predicates)
            {
                if (!predicate.Evaluate(Name, value, summary))
                {
                    return false;
                }
            }
            var result = PatchMethod?.Invoke(value);
            if (result == "remove")
            {
                removed = true;
                summary.RemovedAsset(Name);
            }
            else
            {
                summary.Apply(Name);
            }
        }
        catch (Exception e)
        {
            errored = true;
            summary.Error(Name, e);
            return false;
        }

        return true;
    }

    private string NormalizeId(string id)
    {
        if (id.Contains(':')) return id;
        return $"{PatchModId}:{id}";
    }
}
