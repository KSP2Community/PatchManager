using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Missions.UserData;

namespace PatchManager.Missions.Converters;


/// <summary>
/// <see cref="IConverter" /> registered as <c>"Mission"</c>; wraps a mission definition in a <see cref="MissionUserData" />.
/// </summary>
[Converter("Mission")]
public class MissionConverter : IConverter
{
    /// <inheritdoc />
    public DynValue FromJson(JToken json) =>
        json == null
            ? DynValue.Nil
            : MoonSharp.Interpreter.UserData.Create(new MissionUserData(json));

    /// <inheritdoc />
    public JToken ToJson(DynValue value)
        => value.Type == DataType.Nil
            ? null
            : ((MissionUserData)value.UserData.Object).Token;
}
