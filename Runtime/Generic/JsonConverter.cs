using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;

namespace PatchManager.Generic;

[Converter("JSON")]
public class JsonConverter : IConverter
{
    public DynValue FromJson(JToken json)
    {
        if (json == null) return DynValue.Nil;
        return JsonUserData.GetFromJToken(json);
    }

    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return JsonUserData.GetJTokenForDynValue(value);
    }
}