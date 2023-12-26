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
    
    /// <summary>
    /// Gets the relative path to a working directory with both paths expressed as strings
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="workingDirectory"></param>
    /// <returns></returns>
    public static string MakeRelativePathTo(this string fullPath, string workingDirectory)
    {
        string result = string.Empty;
        int offset;

        // this is the easy case.  The file is inside of the working directory.
        if( fullPath.StartsWith(workingDirectory) )
        {
            return fullPath.Substring(workingDirectory.Length + 1);
        }

        // the hard case has to back out of the working directory
        string[] baseDirs = workingDirectory.Split(':', '\\', '/');
        string[] fileDirs = fullPath.Split(':', '\\', '/');

        // if we failed to split (empty strings?) or the drive letter does not match
        if( baseDirs.Length <= 0 || fileDirs.Length <= 0 || baseDirs[0] != fileDirs[0] )
        {
            // can't create a relative path between separate harddrives/partitions.
            return fullPath;
        }

        // skip all leading directories that match
        for (offset = 1; offset < baseDirs.Length; offset++)
        {
            if (baseDirs[offset] != fileDirs[offset])
                break;
        }

        // back out of the working directory
        for (int i = 0; i < (baseDirs.Length - offset); i++)
        {
            result += "..\\";
        }

        // step into the file path
        for (int i = offset; i < fileDirs.Length-1; i++)
        {
            result += fileDirs[i] + "\\";
        }

        // append the file
        result += fileDirs[fileDirs.Length - 1];

        return result;
    }
}