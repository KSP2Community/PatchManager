using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;

namespace PatchManager.Generic;

/// <summary>
/// Generic <see cref="IConverter" /> registered as <c>"JSON"</c>; wraps any JSON token in a plain
/// <see cref="JsonUserData" /> for Lua scripts to manipulate.
/// </summary>
[Converter("JSON")]
public class JsonConverter : IConverter
{
    /// <inheritdoc />
    public DynValue FromJson(JToken json)
    {
        if (json == null) return DynValue.Nil;
        return JsonUserData.GetFromJToken(json);
    }

    /// <inheritdoc />
    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return JsonUserData.GetJTokenForDynValue(value);
    }
}
