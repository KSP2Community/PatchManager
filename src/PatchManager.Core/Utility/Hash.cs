using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace PatchManager.Core.Utility;

/// <summary>
/// Hashing utility class.
/// </summary>
public static class Hash
{
    private static readonly MD5 Md5 = MD5.Create();

    /// <summary>
    /// Gets the MD5 hash of the input string.
    /// </summary>
    /// <param name="input">Input string.</param>
    /// <returns>MD5 hash of the input string.</returns>
    public static string FromString(string input)
    {
        var hash = Md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return ByteArrayToString(hash);
    }

    /// <summary>
    /// Gets the MD5 hash of the input file.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <returns>MD5 hash of the input file.</returns>
    public static string FromFile(string path)
    {
        var hash = Md5.ComputeHash(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        return ByteArrayToString(hash);
    }

    /// <summary>
    /// Gets the MD5 hash of the input JSON-serializable object.
    /// </summary>
    /// <param name="input">JSON-serializable object.</param>
    /// <returns>MD5 hash of the JSON-serializable object.</returns>
    public static string FromJsonObject(object input)
    {
        var json = JsonConvert.SerializeObject(input, Formatting.None);
        return FromString(json);
    }

    private static string ByteArrayToString(byte[] array)
    {
        var hashString = BitConverter.ToString(array).Replace("-", "");
        return hashString;
    }
}