using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Planets.UserData;

namespace PatchManager.Planets.Converters;

/// <summary>
/// <see cref="IConverter" /> registered as <c>"Cloud"</c>; wraps a volume-cloud configuration in a
/// <see cref="VolumeCloudUserData" /> for patch scripts.
/// </summary>
[Converter("Cloud")]
public class VolumeCloudConverter : IConverter
{
    /// <inheritdoc />
    public DynValue FromJson(JToken json)
    {
        if (json == null)  return DynValue.Nil;
        return MoonSharp.Interpreter.UserData.Create(new VolumeCloudUserData(json));
    }

    /// <inheritdoc />
    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return ((VolumeCloudUserData)value.UserData.Object).Token;
    }
}
