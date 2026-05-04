using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching;

/// <summary>
/// A new asset queued for creation by a Lua patch script.
/// </summary>
public class LuaAsset
{
    /// <summary>
    /// The converter currently producing JSON for <see cref="CurrentValue" />. Replaced when a chained patch
    /// switches to a different converter.
    /// </summary>
    public IConverter ConverterInstance;

    /// <summary>
    /// The asset's current Lua-facing value. Initialized at creation time and replaced by each patch that
    /// runs against this asset.
    /// </summary>
    public DynValue CurrentValue;

    /// <summary>
    /// The addressables label the asset is tagged with for group-based loading.
    /// </summary>
    public string Label;

    /// <summary>
    /// The addressables address of the asset (globally unique).
    /// </summary>
    public string Name;
}
