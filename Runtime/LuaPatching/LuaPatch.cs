using System;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace PatchManager.LuaPatching;

/// <summary>
/// A registered patch operation: which converter, which addressables target, what callback to run, and at which stage.
/// </summary>
[MoonSharpUserData]
public class LuaPatch
{
    /// <summary>
    /// The converter that produces the Lua value the patch operates on and serializes the result back to JSON.
    /// </summary>
    public IConverter ConverterInstance;

    /// <summary>
    /// The user-supplied callback that runs against each matched asset.
    /// </summary>
    /// <remarks>
    /// Returns <c>"remove"</c> (case-insensitive) to delete the asset, or <c>null</c> to keep it. The callback
    /// mutates the wrapped JSON in place; its non-removal return value is otherwise unused.
    /// </remarks>
    public Func<DynValue, string> PatchMethod;

    /// <summary>
    /// The addressables label whose assets this patch targets.
    /// </summary>
    public string Label;

    /// <summary>
    /// The addressables address pattern this patch targets, or <c>null</c> to match every asset in <see cref="Label" />.
    /// </summary>
    /// <remarks>
    /// Supports <c>*</c> and <c>?</c> wildcards, expanded by <see cref="Universe.MatchesPattern" />.
    /// </remarks>
    [CanBeNull] public string Name;

    /// <summary>
    /// The stage name that orders this patch relative to others. Defaults to the host mod's ID.
    /// </summary>
    public string Stage;

    /// <summary>
    /// Precomputed stage priority used by sorting
    /// </summary>
    [MoonSharpHidden] public ulong StagePriority;
    
    /// <summary>
    /// Runs this patch as the first in a chain, lifting the raw JSON through <see cref="ConverterInstance" /> first.
    /// </summary>
    /// <param name="json">The asset's raw JSON.</param>
    /// <returns>The patched value, or <see cref="DynValue.Nil" /> if the asset was deleted.</returns>
    public DynValue ApplyFirst(JToken json)
    {
        var instance = ConverterInstance.FromJson(json);
        if (instance.IsNil()) return DynValue.Nil;
        var result = PatchMethod(instance);
        if (result != null && result.Equals("remove", StringComparison.OrdinalIgnoreCase)) return DynValue.Nil;
        return instance;
    }

    /// <summary>
    /// Runs this patch against a value already produced by an earlier patch in the chain (same converter).
    /// </summary>
    /// <param name="previous">The value produced by the previous patch.</param>
    /// <returns>The same value (patches mutate in place), or <see cref="DynValue.Nil" /> if the asset was deleted.</returns>
    public DynValue ApplyInChain(DynValue previous)
    {
        if (previous.IsNil()) return previous;
        var result = PatchMethod(previous);
        if (result != null && result.Equals("remove", StringComparison.OrdinalIgnoreCase)) return DynValue.Nil;
        return previous;
    }

    /// <summary>
    /// Sets <see cref="Stage" /> to the given stage name and returns this patch for chaining.
    /// </summary>
    /// <param name="stage">The stage to schedule the patch in.</param>
    /// <returns>This patch.</returns>
    public LuaPatch OnStage(string stage)
    {
        Stage = stage;
        return this;
    }
}
