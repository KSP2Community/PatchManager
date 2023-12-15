using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.SassyPatching.Builtins;


/// <summary>
/// This contains all the builtin methods for converting between types
/// </summary>
[SassyLibrary("builtin","type-conversion"),PublicAPI]
public class TypeConversion
{
    /// <summary>
    /// Used in a patch to convert a value to a boolean
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>A boolean form of the value</returns>
    [SassyMethod("to-bool")]
    public static bool ToBoolean(DataValue value) => value.Truthy;


    /// <summary>
    /// Used in a patch to convert a value to a real
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The value interpreted as a real</returns>
    [SassyMethod("to-real")]
    public static double ToReal(DataValue value)
    {
        if (value.IsInteger) return value.Integer;
        if (value.IsReal) return value.Real;
        if (value.IsString) return double.Parse(value.String);
        throw new InvalidCastException($"Cannot convert value of type {value.Type.ToString().ToLowerInvariant()} to real");
    }
    
    /// <summary>
    /// Used in a patch to convert a value to a real
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The value interpreted as a real</returns>
    [SassyMethod("to-integer")]
    public static long ToInteger(DataValue value)
    {
        if (value.IsInteger) return value.Integer;
        if (value.IsReal) return (long)value.Real;
        if (value.IsString) return long.Parse(value.String);
        throw new InvalidCastException($"Cannot convert value of type {value.Type.ToString().ToLowerInvariant()} to integer");
    }
    
    /// <summary>
    /// Used in a patch to convert a value to a string
    /// </summary>
    /// <param name="v">The value</param>
    /// <returns>A string form of the value</returns>
    [SassyMethod("to-string")]
    public static string ToString(DataValue value)
    {
        return value.IsString ? value.String : value.ToString();
    }

    [SassyMethod("dictionary.to-list")]
    public static List<List<DataValue>> ToList(Dictionary<string, DataValue> dict) => dict.Select(kv =>
        new List<DataValue>()
        {
            DataValue.From(kv.Key),
            kv.Value
        }).ToList();

    [SassyMethod("list.to-dictionary")]
    public static Dictionary<string, DataValue> ToDictionary(List<List<DataValue>> list)
    {
        var newDict = new Dictionary<string, DataValue>();
        foreach (var value in list)
        {
            if (value.Count != 2)
                throw new InvalidCastException("All values in $list must be 2-pairs of key, value");
            if (!value[0].IsString)
                throw new InvalidCastException("Dictionary keys must be strings");
            newDict[value[0].String] = value[1];
        }
        return newDict;
    }

    [SassyMethod("string.to-list")]
    public static List<string> ToList([SassyName("string")] string str) => str.Select(x => $"{x}").ToList();

    }