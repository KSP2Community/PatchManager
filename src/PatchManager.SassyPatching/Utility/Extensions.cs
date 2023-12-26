using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Newtonsoft.Json;
using SassyPatchGrammar;

namespace PatchManager.SassyPatching.Utility;

internal static class Extensions
{
    public static string Unescape(this string @this) => Regex.Unescape(@this.Substring(1, @this.Length - 2));

    public static string TrimFirst(this string @this) => @this.Substring(1);

    public static Coordinate GetCoordinate(this ParserRuleContext @this) =>
        new(@this.Start.TokenSource.SourceName, @this.Start.Line, @this.Start.Column);

    public static bool MatchesPattern(this string @this, string pattern) =>
        Regex.IsMatch(@this, pattern.Replace("*", ".*").Replace("?", ".?"));

    public static string Escape(this string @this) => JsonConvert.ToString(@this);

    public static string GetStringValue(this sassy_parser.Sassy_stringContext ctx)
    {
        if (ctx is sassy_parser.Quoted_stringContext quotedStringContext)
        {
            return quotedStringContext.STRING().GetText().Unescape();
        }

        var unquotedStringContext = ctx as sassy_parser.Unquoted_stringContext;
        return unquotedStringContext!.ELEMENT().GetText();
    }
}