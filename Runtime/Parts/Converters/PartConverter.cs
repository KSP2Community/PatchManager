using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Parts.UserData;

namespace PatchManager.Parts.Converters;

[Converter("Part")]
public class PartConverter : IConverter
{
    public DynValue FromJson(JToken json) =>
        json == null
            ? DynValue.Nil
            : MoonSharp.Interpreter.UserData.Create(new PartUserData(json));

    public JToken ToJson(DynValue value)
        => value.Type == DataType.Nil
            ? null
            : ((PartUserData)value.UserData.Object).FullToken.ToString(Formatting.Indented);
}