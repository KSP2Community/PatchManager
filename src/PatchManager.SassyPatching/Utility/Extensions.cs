using System.Text.RegularExpressions;
using Antlr4.Runtime;
using SassyPatchGrammar;

namespace PatchManager.SassyPatching.Utility;

internal static class Extensions
{
    public static string Unescape(this string @this)
    {
        return Regex.Unescape(@this.Substring(1, @this.Length - 2));
    }

    public static string TrimFirst(this string @this)
    {
        return @this.Substring(1);
    }

    public static Coordinate GetCoordinate(this ParserRuleContext @this)
    {
        return new Coordinate(@this.Start.TokenSource.SourceName, @this.Start.Line, @this.Start.Column);
    }

    public static bool MatchesPattern(this string @this, string pattern)
    {
        return Regex.IsMatch(@this, pattern.Replace("*", ".*").Replace("?", ".?"));
    }

}