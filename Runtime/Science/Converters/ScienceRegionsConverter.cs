using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Science.UserData;

namespace PatchManager.Science.Converters;

[Converter("ScienceRegions")]
public class ScienceRegionsConverter : IConverter
{
    public DynValue FromJson(JToken json)
    {
        if (json == null) return DynValue.Nil;
        return MoonSharp.Interpreter.UserData.Create(new ScienceRegionsUserData(json));
    }

    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return ((ScienceRegionsUserData)value.UserData.Object).FullToken;
    }
}