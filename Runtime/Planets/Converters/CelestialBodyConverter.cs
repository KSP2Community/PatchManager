using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Planets.UserData;

namespace PatchManager.Planets.Converters;

/// <summary>
/// <see cref="IConverter" /> registered as <c>"Planet"</c>; wraps celestial body JSON envelopes in a
/// <see cref="CelestialBodyUserData" /> for patching of the inner <c>data</c> subtree.
/// </summary>
[Converter("Planet")]
public class CelestialBodyConverter : IConverter
{
    /// <inheritdoc />
    public DynValue FromJson(JToken json)
    {
        if (json == null)  return DynValue.Nil;
        return MoonSharp.Interpreter.UserData.Create(new CelestialBodyUserData(json));
    }

    /// <inheritdoc />
    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return ((CelestialBodyUserData)value.UserData.Object).FullToken;
    }
}
