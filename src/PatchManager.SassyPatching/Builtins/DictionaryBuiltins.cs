using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.SassyPatching.Builtins;
/// <summary>
/// Object/dictionary builtins
/// </summary>
[SassyLibrary("builtin","dictionary")]
[PublicAPI]
public class DictionaryBuiltins
{
    [SassyMethod("dictionary.set")]
    public static Dictionary<string, DataValue> Set(Dictionary<string, DataValue> from, string key, DataValue value)
    {
        var result = new Dictionary<string, DataValue>(from);
        result[key] = value;
        return result;
    }
}