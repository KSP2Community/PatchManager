using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Parts.UserData;

namespace PatchManager.Parts.Converters;

/// <summary>
/// <see cref="IConverter" /> registered as <c>"Part"</c>; wraps a part definition envelope in a <see cref="PartUserData" />.
/// </summary>
[Converter("Part")]
public class PartConverter : IConverter
{
    /// <inheritdoc />
    public DynValue FromJson(JToken json) =>
        json == null
            ? DynValue.Nil
            : MoonSharp.Interpreter.UserData.Create(new PartUserData(json));

    /// <inheritdoc />
    public JToken ToJson(DynValue value)
        => value.Type == DataType.Nil
            ? null
            : ((PartUserData)value.UserData.Object).FullToken;
}
