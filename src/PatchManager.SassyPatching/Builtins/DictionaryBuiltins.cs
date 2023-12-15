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

    [SassyMethod("dictionary.merge")]
    public static Dictionary<string, DataValue> Merge(
        Dictionary<string, DataValue> original,
        Dictionary<string, DataValue> toMerge
    )
    {
        var newDictionary = new Dictionary<string, DataValue>(original);
        foreach (var (key, value) in toMerge)
        {
            newDictionary[key] = value;
        }
        return newDictionary;
    }

    [SassyMethod("dictionary.keys")]
    public static List<string> Keys(Dictionary<string, DataValue> dictionary) => dictionary.Keys.ToList();

    [SassyMethod("dictionary.values")]
    public static List<DataValue> Values(Dictionary<string, DataValue> dictionary) => dictionary.Values.ToList();


    [SassyMethod("dictionary.remove")]
    public static Dictionary<string, DataValue> Remove(Dictionary<string, DataValue> from, string key) =>
        from.Where(kv => kv.Key != key).ToDictionary(kv => kv.Key,kv=>kv.Value);
}