using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Newtonsoft.Json;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Nodes.Expressions;
using SassyPatchGrammar;
using Environment = PatchManager.SassyPatching.Execution.Environment;

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

    private static object GetResult(this string variable, Environment environment)
    {
        var lexer = new sassy_lexer(CharStreams.fromString(variable));
        var lexerErrorGenerator = new Universe.LexerListener("Interpolated string",
            environment.GlobalEnvironment.Universe.MessageLogger);
        lexer.AddErrorListener(lexerErrorGenerator);
        if (lexerErrorGenerator.Errored)
        {
            throw new Exception("Lexer errors detected");
        }
        
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new sassy_parser(tokenStream);
        var parserErrorGenerator = new Universe.ParserListener("Interpolated string",
            environment.GlobalEnvironment.Universe.MessageLogger);
        parser.AddErrorListener(parserErrorGenerator);
        var expressionContext = parser.expression();
        if (parserErrorGenerator.Errored)
            throw new Exception("parser errors detected");
        var tokenTransformer = new Transformer(msg => throw new Exception(msg));
        var ctx = tokenTransformer.Visit(expressionContext) as Expression;
        var result = ctx!.Compute(environment);
        return result.IsString ? result.String : result.ToString();
    }
    
    public static string Interpolate(this string toBeInterpolated, Environment environment)
    {
        List<object> arguments = [];
        var index = 0;
        StringBuilder format = new();
        var inFormat = false;
        var currentString = "";
        var lookForStart = false;
        
        foreach (var character in toBeInterpolated)
        {
            if (inFormat)
            {
                if (character is '}')
                {
                    format.Append(currentString.GetResult(environment));
                    inFormat = false;
                }
                else
                {
                    currentString += character;
                }
            }
            else
            {
                if (lookForStart)
                {
                    if (character == '{')
                    {
                        currentString = "";
                        inFormat = true;
                    }
                    else if (character == '#')
                    {
                        format.Append('#');
                    }
                    else
                    {
                        format.Append('#');
                        format.Append(character);
                    }

                    lookForStart = false;
                }
                if (character == '#')
                {
                    lookForStart = true;
                }
                else
                {
                    format.Append(currentString);
                }
            }
        }

        if (lookForStart)
        {
            format.Append('#');
        }

        if (inFormat)
        {
            throw new Exception("Unterminated interpolated string");
        }

        return format.ToString();
    }
}