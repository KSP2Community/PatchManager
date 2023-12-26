using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Builtins;

/// <summary>
/// This is the list library used by sassy patching, every method that is list specific in it is prepended w/ list. to prevent overloading
/// </summary>
[SassyLibrary("builtin", "list")]
[PublicAPI]
public class ListBuiltins
{
    /// <summary>
    /// Copies a list and appends a value onto the copy and returns the copy
    /// </summary>
    /// <param name="list">The list to copy</param>
    /// <param name="value">The value to append</param>
    /// <returns>The new list w/ the value appended to it</returns>
    [SassyMethod("list.append")]
    public static List<DataValue> Append(List<DataValue> list, DataValue value) => new(list) { value };

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

    private static int DefaultComparison(DataValue a, DataValue b)
    {
        if (a.IsInteger && b.IsInteger)
        {
            return a.Integer < b.Integer ? -1 : a.Integer == b.Integer ? 0 : 1;
        }

        if (a.IsReal && b.IsInteger)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return a.Real < b.Integer ? -1 : a.Real == b.Integer ? 0 : 1;
        }

        if (a.IsInteger && b.IsReal)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return a.Integer < b.Real ? -1 : a.Integer == b.Real ? 0 : 1;
        }

        if (a.IsReal && b.IsReal)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return a.Real < b.Real ? -1 : a.Real == b.Real ? 0 : 1;
        }

        if (a.IsString && b.IsString)
        {
            return string.Compare(a.String, b.String, StringComparison.Ordinal);
        }

        throw new Exception(
            $"Cannot compare a value of type {a.Type.ToString().ToLowerInvariant()} and {b.Type.ToString().ToLowerInvariant()}");
    }

    private static int InvokeComparison(Environment env, PatchFunction comparator, DataValue a, DataValue b)
    {
        var result = comparator.Execute(env, new List<PatchArgument>
        {
            new PatchArgument
            {
                ArgumentDataValue = a
            },
            new PatchArgument
            {
                ArgumentDataValue = b
            },
        });
        if (result.IsInteger)
        {
            // A simple are these less 
            return (int)result.Integer;
        }

        if (result.IsReal)
        {
            return (int)result.Real;
        }

        throw new TypeConversionException(result.Type.ToString().ToLowerInvariant(), "integer");
    }

    /// <summary>
    /// Sorts a list, using a comparison function if one is provided otherwise uses a default comparison algorithm
    /// </summary>
    /// <param name="env"></param>
    /// <param name="list"></param>
    /// <param name="comparator">The function to sort with, default null (due to closures not existing in the program yet), follows the C# comparison function</param>
    /// <returns></returns>
    [SassyMethod("list.sort")]
    public static List<DataValue> Sort(Environment env, List<DataValue> list, PatchFunction comparator = null)
    {
        var copy = new List<DataValue>(list);
        Comparison<DataValue> comparison =
            comparator != null ? (x, y) => InvokeComparison(env, comparator, x, y) : DefaultComparison;
        copy.Sort(comparison);
        return copy;
    }


    /// <summary>
    /// Copy a list, apply a function over the copy and return that copy
    /// </summary>
    /// <param name="env">The environment this is being ran in</param>
    /// <param name="list"></param>
    /// <param name="closure">The function to apply to the list</param>
    /// <returns>The copy w/ the function applied over it</returns>
    [SassyMethod("list.map")]
    public static List<DataValue> Map(
        Environment env,
        List<DataValue> list,
        PatchFunction closure
    ) =>
        list.Select(value => closure.Execute(env,
                new List<PatchArgument>
                {
                    new()
                    {
                        ArgumentDataValue = value
                    }
                }))
            .ToList();

    /// <summary>
    /// Copy a list, apply a function over the copy to filter the lists
    /// </summary>
    /// <param name="env">The environment this is being ran in</param>
    /// <param name="list"></param>
    /// <param name="closure">The function to apply to the list</param>
    /// <returns>The copy w/ the function applied over it</returns>
    [SassyMethod("list.filter")]
    public static List<DataValue> Filter(
        Environment env,
        List<DataValue> list,
        PatchFunction closure
    ) =>
        list.Where(value => closure.Execute(env,
                new List<PatchArgument>
                {
                    new()
                    {
                        ArgumentDataValue = value
                    }
                }).Truthy)
            .ToList();

    /// <summary>
    /// Aggregate a list into one value
    /// </summary>
    /// <param name="env">The environment of the list</param>
    /// <param name="list"></param>
    /// <param name="initialValue">The starting value of the aggregation</param>
    /// <param name="closure">The function to apply to take the aggregate</param>
    /// <returns>The aggregate value</returns>
    [SassyMethod("list.reduce")]
    public static DataValue Reduce(
        Environment env,
        List<DataValue> list,
        [SassyName("initial-value")] DataValue initialValue,
        PatchFunction closure
    ) =>
        list.Aggregate(initialValue,
            (
                current,
                value
            ) => closure.Execute(env,
                new List<PatchArgument>
                {
                    new()
                    {
                        ArgumentDataValue = current
                    },
                    new()
                    {
                        ArgumentDataValue = value
                    }
                }));

    /// <summary>
    /// Gets the length of a list
    /// </summary>
    /// <param name="list">The list</param>
    /// <returns>The length of the list</returns>
    [SassyMethod("list.length")]
    public static int Length(List<DataValue> list) => list.Count;

    [SassyMethod("list.join")]
    public static string ToString(List<DataValue> list, string separator = "") =>
        string.Join("", list.Select(x => x.IsString ? x.String : x.ToString()));

    [SassyMethod("list.remove")]
    public static List<DataValue> Remove(List<DataValue> list, [SassyName("to-remove")] DataValue toRemove) =>
        list.Where(x => x != toRemove).ToList();

    [SassyMethod("list.remove-all")]
    public static List<DataValue> RemoveAll(List<DataValue> list, [SassyName("to-remove")] List<DataValue> toRemove) =>
        list.Where(x => toRemove.All(y => y != x)).ToList();

    [SassyMethod("list.slice")]
    public static List<DataValue> Slice(List<DataValue> list, int start, int end) =>
        list.Skip(start).Take(end-start).ToList();

}