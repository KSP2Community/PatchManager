using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Science.UserData;

namespace PatchManager.Science.Converters;

/// <summary>
/// <see cref="IConverter" /> registered as <c>"Discoverables"</c>; wraps a science region's discoverables envelope
/// in a <see cref="DiscoverablesUserData" />.
/// </summary>
[Converter("Discoverables")]
public class DiscoverablesConverter : IConverter
{
    /// <inheritdoc />
    public DynValue FromJson(JToken json)
    {
        if (json == null) return DynValue.Nil;
        return MoonSharp.Interpreter.UserData.Create(new DiscoverablesUserData(json));
    }

    /// <inheritdoc />
    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return ((DiscoverablesUserData)value.UserData.Object).FullToken;
    }
}
