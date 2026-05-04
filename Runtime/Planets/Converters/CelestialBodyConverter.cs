using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Planets.UserData;

namespace PatchManager.Planets.Converters;

[Converter("Planet")]
public class CelestialBodyConverter : IConverter
{
    public DynValue FromJson(JToken json)
    {
        if (json == null)  return DynValue.Nil;
        return MoonSharp.Interpreter.UserData.Create(new CelestialBodyUserData(json));
    }

    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return ((CelestialBodyUserData)value.UserData.Object).FullToken;
    }
}