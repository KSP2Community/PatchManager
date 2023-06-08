using PatchManager.SassyPatching.Tests.Validators;
using PatchManager.SassyPatching.Tests.Validators.Expressions;
using PatchManager.SassyPatching.Tests.Validators.Statements;

namespace PatchManager.SassyPatching.Tests;

public class ParsingTests
{

    private Transformer _tokenTransformer;
    
    [SetUp]
    public void Setup()
    {
        _tokenTransformer = new Transformer(Assert.Fail);
    }


    private SassyPatch Parse(string testPatch)
    {
        var charStream = CharStreams.fromString(testPatch);
        var lexer = new sassy_lexer(charStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new sassy_parser(tokenStream);
        var patchContext = parser.patch();
        _tokenTransformer.Errored = false;
        var patch = _tokenTransformer.Visit(patchContext) as SassyPatch;
        Assert.That(_tokenTransformer.Errored, Is.False);
        return patch!;
    }
        
    [Test]
    public void SimplePatch()
    {
        const string patch = 
@"
$variable: 5;
";
        var parsed = Parse(patch);
        var validator = new PatchValidator
        {
            new VarDeclValidator
            {
                Variable = "variable",
                Value = new ValueValidator
                {
                    StoredValue = 5
                }
            }
        };

        Assert.That(validator.Validate(parsed), Is.True);
    }
}