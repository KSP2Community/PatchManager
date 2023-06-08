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
}