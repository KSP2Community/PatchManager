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
    public static Dictionary<string, DataValue> Set(Dictionary<string, DataValue> dict, string key, DataValue value)
    {
        var result = new Dictionary<string, DataValue>(dict);
        if (value.IsDeletion)
        {
            result.Remove(key);
        }
        else
        {
            result[key] = value;
        }
        return result;
    }

    [SassyMethod("dictionary.merge")]
    public static Dictionary<string, DataValue> Merge(
        [SassyName("original-dict")] Dictionary<string, DataValue> original,
        [SassyName("new-dict")] Dictionary<string, DataValue> toMerge
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
    public static List<string> Keys(Dictionary<string, DataValue> dict) => dict.Keys.ToList();

    [SassyMethod("dictionary.values")]
    public static List<DataValue> Values(Dictionary<string, DataValue> dict) => dict.Values.ToList();


    [SassyMethod("dictionary.remove")]
    public static Dictionary<string, DataValue> Remove(Dictionary<string, DataValue> dict, string key) =>
        dict.Where(kv => kv.Key != key).ToDictionary(kv => kv.Key,kv=>kv.Value);

    [SassyMethod("dictionary.count")]
    public static int Count(Dictionary<string, DataValue> dict) => dict.Count;
}