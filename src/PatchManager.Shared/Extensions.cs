namespace PatchManager.Shared;

/// <summary>
/// Utility extension methods for various types.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Deconstructs a <see cref="KeyValuePair{TKey,TValue}"/> into its key and value.
    /// </summary>
    /// <param name="keyValuePair">Key-value pair to deconstruct.</param>
    /// <param name="key">The deconstructed key.</param>
    /// <param name="value">The deconstructed value.</param>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    public static void Deconstruct<TKey, TValue>(
        this KeyValuePair<TKey, TValue> keyValuePair,
        out TKey key,
        out TValue value
    )
    {
        key = keyValuePair.Key;
        value = keyValuePair.Value;
    }

    /// <summary>
    /// Adds the contents of an <see cref="IEnumerable{T}"/> of type <see cref="KeyValuePair{TKey,TValue}"/>
    /// to a <see cref="Dictionary{TKey,TValue}"/>.
    /// </summary>
    /// <param name="target">Target dictionary to add to.</param>
    /// <param name="source">Source enumerable to add from.</param>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    public static void AddRangeUnique<TKey, TValue>(
        this Dictionary<TKey, TValue> target,
        IEnumerable<KeyValuePair<TKey, TValue>> source
    )
    {
        foreach (var kvp in source)
        {
            if (target.ContainsKey(kvp.Key))
            {
                continue;
            }

            target.Add(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Adds the contents of an <see cref="IEnumerable{T}"/> of type <see cref="KeyValuePair{TKey,IEnumerable}"/>
    /// to a <see cref="Dictionary{TKey,IEnumerable}"/>. If the key already exists, the contents of the value
    /// enumerable will be added to the existing value enumerable.
    /// </summary>
    /// <param name="target">Target dictionary to add to.</param>
    /// <param name="source">Source enumerable to add from.</param>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value <see cref="IEnumerable{T}"/>.</typeparam>
    public static void AddRangeMerge<TKey, TValue>(
        this Dictionary<TKey, IEnumerable<TValue>> target,
        IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> source
    )
    {
        foreach (var kvp in source)
        {
            if (target.ContainsKey(kvp.Key))
            {
                target[kvp.Key] = target[kvp.Key].Concat(kvp.Value);
                continue;
            }

            target.Add(kvp.Key, kvp.Value);
        }
    }
}