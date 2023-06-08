using JetBrains.Annotations;

namespace PatchManager.Core.Assets;

/// <summary>
/// A class that holds all the addresses of the patched files.
/// </summary>
[PublicAPI]
public static class Addresses
{
    internal static readonly Dictionary<object, List<string>> RegisteredAddresses = new();

    /// <summary>
    /// Register a patched asset under a label.
    /// </summary>
    /// <param name="key">The label to register the assets under.</param>
    /// <param name="filename">Filename of the patched asset.</param>
    public static void Register(object key, string filename)
    {
        InitializeKey(key);
        RegisteredAddresses[key].Add(filename);
    }

    /// <summary>
    /// Register multiple patched assets under a label.
    /// </summary>
    /// <param name="key">The label to register the assets under.</param>
    /// <param name="filenames">Filenames of the patched assets.</param>
    public static void Register(object key, IEnumerable<string> filenames)
    {
        InitializeKey(key);
        RegisteredAddresses[key].AddRange(filenames);
    }

    /// <summary>
    /// Register multiple patched assets under their labels from a dictionary.
    /// </summary>
    /// <param name="files">Dictionary of labels and the filenames of their assigned assets.</param>
    public static void Register(Dictionary<object, string> files)
    {
        foreach (var (key, filename) in files)
        {
            Register(key, filename);
        }
    }

    private static void InitializeKey(object key)
    {
        if (!RegisteredAddresses.ContainsKey(key))
        {
            RegisteredAddresses[key] = new List<string>();
        }
    }
}