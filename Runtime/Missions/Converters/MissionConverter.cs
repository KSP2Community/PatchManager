using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Missions.UserData;

namespace PatchManager.Missions.Converters;


[Converter("Mission")]
public class MissionConverter : IConverter
{
    public DynValue FromJson(JToken json) =>
        json == null
            ? DynValue.Nil
            : MoonSharp.Interpreter.UserData.Create(new MissionUserData(json));

    public JToken ToJson(DynValue value)
        => value.Type == DataType.Nil
            ? null
            : ((MissionUserData)value.UserData.Object).Token.ToString(Formatting.Indented);
}