using JetBrains.Annotations;

namespace PatchManager.Core;

[PublicAPI]
public static class Addresses
{
    internal static readonly Dictionary<object, List<string>> RegisteredAddresses = new();

    public static void Register(object key, string filename)
    {
        InitializeKey(key);
        RegisteredAddresses[key].Add(filename);
    }

    public static void Register(object key, IEnumerable<string> filenames)
    {
        InitializeKey(key);
        RegisteredAddresses[key].AddRange(filenames);
    }

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