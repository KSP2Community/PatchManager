using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Planets.UserData;

namespace PatchManager.Planets.Converters;

[Converter("Galaxy")]
public class GalaxyConverter : IConverter
{
    public DynValue FromJson(JToken json)
    {
        if (json == null)  return DynValue.Nil;
        return MoonSharp.Interpreter.UserData.Create(new GalaxyUserData(json));
    }

    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return ((GalaxyUserData)value.UserData.Object).Token;
    }
}