using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Planets.UserData;

namespace PatchManager.Planets.Converters;

/// <summary>
/// <see cref="IConverter" /> registered as <c>"Galaxy"</c>; wraps a galaxy definition in a
/// <see cref="GalaxyUserData" /> for patch scripts.
/// </summary>
[Converter("Galaxy")]
public class GalaxyConverter : IConverter
{
    /// <inheritdoc />
    public DynValue FromJson(JToken json)
    {
        if (json == null)  return DynValue.Nil;
        return MoonSharp.Interpreter.UserData.Create(new GalaxyUserData(json));
    }

    /// <inheritdoc />
    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return ((GalaxyUserData)value.UserData.Object).Token;
    }
}
