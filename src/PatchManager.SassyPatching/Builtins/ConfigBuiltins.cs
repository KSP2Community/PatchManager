using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Execution;

namespace PatchManager.SassyPatching.Builtins;

[SassyLibrary("builtin", "config")]
public class ConfigBuiltins
{
    [SassyMethod("get-config")]
    public static DataValue GetConfig(Universe universe, string label, string name) =>
        universe.Configs.TryGetValue(label, out var labelDict)
            ? labelDict.TryGetValue(name, out var result) ? result : new DataValue(DataValue.DataType.None)
            : new DataValue(DataValue.DataType.None);

    [SassyMethod("get-configs")]
    public static DataValue GetConfigs(Universe universe, string label) =>
        universe.Configs.TryGetValue(label, out var labelDict) ? labelDict : new Dictionary<string, DataValue>();
}