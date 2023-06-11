using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;

namespace PatchManager.SassyPatching.Builtins;

/// <summary>
/// This is the list library used by sassy patching, every method that is list specific in it is prepended w/ list. to prevent overloading
/// </summary>
[SassyLibrary("builtin","list")]
[PublicAPI]
public class ListBuiltins
{
    /// <summary>
    /// Copies a list and appends a value onto the copy and returns the copy
    /// </summary>
    /// <param name="list">The list to copy</param>
    /// <param name="dataValue">The value to append</param>
    /// <returns>The new list w/ the value appended to it</returns>
    [SassyMethod("list.append")]
    public static List<DataValue> Append(List<DataValue> list, DataValue dataValue) => new(list) { dataValue };

    /// <summary>
    /// Copies a list and appends a list of values onto the copy and returns the copy
    /// Same as the `+` operator between lists
    /// </summary>
    /// <param name="list">The list to copy</param>
    /// <param name="values">The list of values to append</param>
    /// <returns>The new list w/ the values appended to it</returns>
    [SassyMethod("list.append-all")]
    public static List<DataValue> AppendAll(List<DataValue> list, List<DataValue> values)
    {
        var result = new List<DataValue>(list);
        result.AddRange(values);
        return result;
    }

    /// <summary>
    /// Creates a new list out 
    /// </summary>
    /// <param name="varArgs"></param>
    /// <returns></returns>
    [SassyMethod("list.create")]
    public static List<DataValue> Create([VarArgs] List<DataValue> varArgs)
    {
        return varArgs;
    }
}