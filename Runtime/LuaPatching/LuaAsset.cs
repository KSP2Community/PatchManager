using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching;

/// <summary>
/// Represents a lua asset, created using CreateNew
/// </summary>
public class LuaAsset
{
    /// <summary>
    /// The Converter associated with this newly created asset
    /// </summary>
    public IConverter ConverterInstance;
    /// <summary>
    /// The value that was returned at creation time
    /// </summary>
    public DynValue CurrentValue;
    /// <summary>
    /// 
    /// </summary>
    public string Label;
    public string Name;
}