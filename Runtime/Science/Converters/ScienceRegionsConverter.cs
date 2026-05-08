using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Science.UserData;

namespace PatchManager.Science.Converters;

/// <summary>
/// <see cref="IConverter" /> registered as <c>"ScienceRegions"</c>; wraps a science-region envelope in a
/// <see cref="ScienceRegionsUserData" />.
/// </summary>
[Converter("ScienceRegions")]
public class ScienceRegionsConverter : IConverter
{
    /// <inheritdoc />
    public DynValue FromJson(JToken json)
    {
        if (json == null) return DynValue.Nil;
        return MoonSharp.Interpreter.UserData.Create(new ScienceRegionsUserData(json));
    }

    /// <inheritdoc />
    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return ((ScienceRegionsUserData)value.UserData.Object).FullToken;
    }
}
