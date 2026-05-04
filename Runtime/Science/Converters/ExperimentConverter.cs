using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.Science.UserData;

namespace PatchManager.Science.Converters;

/// <summary>
/// <see cref="IConverter" /> registered as <c>"Experiment"</c>; wraps a science experiment envelope in an
/// <see cref="ExperimentUserData" />.
/// </summary>
[Converter("Experiment")]
public class ExperimentConverter : IConverter
{
    /// <inheritdoc />
    public DynValue FromJson(JToken json)
    {
        if (json == null) return DynValue.Nil;
        return MoonSharp.Interpreter.UserData.Create(new ExperimentUserData(json));
    }

    /// <inheritdoc />
    public JToken ToJson(DynValue value)
    {
        if (value.Type == DataType.Nil) return null;
        return ((ExperimentUserData)value.UserData.Object).FullToken;
    }
}
