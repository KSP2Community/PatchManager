using System.Text.RegularExpressions;

namespace PatchManager.Core.Utility;

internal static class Extensions
{
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