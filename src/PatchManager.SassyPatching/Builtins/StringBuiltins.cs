using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Builtins;

/// <summary>
/// This contains all the builtin methods applying over strings
/// </summary>
[SassyLibrary("builtin","string"),PublicAPI]
public class StringBuiltins
{
    [SassyMethod("string.length")]
    public static int Length([SassyName("string")] string str) => str.Length;

    [SassyMethod("string.reverse")]
    public static string Reverse([SassyName("string")] string str) => string.Join("",str.Reverse());

    [SassyMethod("string.starts-with")]
    public static bool StartsWith([SassyName("string")] string str, [SassyName("other-string")] string otherString) =>
        str.StartsWith(otherString);
    
    
    [SassyMethod("string.ends-with")]
    public static bool EndsWith([SassyName("string")] string str, [SassyName("other-string")] string otherString) =>
        str.EndsWith(otherString);
    
    [SassyMethod("string.contains")]
    public static bool Contains([SassyName("string")] string str, [SassyName("other-string")] string otherString) =>
        str.Contains(otherString);

    [SassyMethod("string.to-codepoint")]
    public static char ToCodePoint([SassyName("string")] string str)
    {
        if (str.Length != 1)
            throw new InvocationException("The length of the argument string must be one");
        return str[0];
    }

    [SassyMethod("codepoint-to-string")]
    public static string CodePointToString([SassyName("codepoint")] char codePoint) => $"{codePoint}";
}