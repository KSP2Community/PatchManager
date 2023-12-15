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
    /// <param name="v">The value</param>
    /// <returns>A boolean form of the value</returns>
    [SassyMethod("to-bool")]
    public static bool ToBoolean(DataValue v)
    {
        return v.Truthy;
    }


    /// <summary>
    /// Used in a patch to convert a value to a real
    /// </summary>
    /// <param name="v">The value</param>
    /// <returns>The value interpreted as a real</returns>
    [SassyMethod("to-real")]
    public static double ToReal(DataValue v)
    {
        if (v.IsInteger) return v.Integer;
        if (v.IsReal) return v.Real;
        if (v.IsString) return double.Parse(v.String);
        throw new InvalidCastException($"Cannot convert value of type {v.Type.ToString().ToLowerInvariant()} to real");
    }
    
    /// <summary>
    /// Used in a patch to convert a value to a real
    /// </summary>
    /// <param name="v">The value</param>
    /// <returns>The value interpreted as a real</returns>
    [SassyMethod("to-integer")]
    public static long ToInteger(DataValue v)
    {
        if (v.IsInteger) return v.Integer;
        if (v.IsReal) return (long)v.Real;
        if (v.IsString) return long.Parse(v.String);
        throw new InvalidCastException($"Cannot convert value of type {v.Type.ToString().ToLowerInvariant()} to integer");
    }
    
    /// <summary>
    /// Used in a patch to convert a value to a string
    /// </summary>
    /// <param name="v">The value</param>
    /// <returns>A string form of the value</returns>
    [SassyMethod("to-string")]
    public static string ToString(DataValue v)
    {
        return v.IsString ? v.String : v.ToString();
    }

    [SassyMethod("dictionary.to-list")]
    public static List<List<DataValue>> ToList(Dictionary<string, DataValue> dictionary) => dictionary.Select(kv =>
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
}