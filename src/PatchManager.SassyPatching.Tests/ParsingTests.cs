using PatchManager.SassyPatching.Tests.Validators;
using PatchManager.SassyPatching.Tests.Validators.Expressions;
using PatchManager.SassyPatching.Tests.Validators.Statements;
using PatchManager.SassyPatching.Tests.Validators.Statements.TopLevel;

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

    private void Match(string patch, ParseValidator validator)
    {
        Assert.That(validator.Validate(Parse(patch)),Is.True);
    }
    
    [Test(TestOf = typeof(Transformer),Author = "Cheese",Description = "Tests single line comments")]
    public void SingleLineComments()
    {
        const string patch = 
            @"
// This is a single line comment
$variable: 5;
";
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

        Match(patch,validator);
    }
    [Test(TestOf = typeof(Transformer),Author = "Cheese",Description = "Tests multi line comments")]
    public void MultiLineComment()
    {
        const string patch = 
            @"
/*
 * This is a multiline comment
 */
$variable: 5;
";
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
        Match(patch, validator);
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an import declaration")]
    public void ImportDeclaration()
    {
        const string patch = @"
@use 'a:b:c';
";
        var validator = new PatchValidator
        {
            new ImportValidator
            {
                Library = "a:b:c"
            }
        };
        Match(patch,validator);
    }
    
        
    [Test(TestOf = typeof(Transformer),Author = "Cheese",Description = "Tests a top level variable declaration")]
    public void TopLevelVariableDeclaration()
    {
        const string patch = 
@"
$variable: 5;
";
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
        Match(patch,validator);
    }
    
    [Test(TestOf = typeof(Transformer),Author = "Cheese",Description = "Tests a stage definition")]
    public void StageDefinition()
    {
        const string patch = 
@"
@define-stage 'create-engines', 42;
";
        var validator = new PatchValidator
        {
            new StageDefinitionValidator
            {
                StageName = "create-engines",
                StagePriority = 42
            }
        };
        Match(patch,validator);
    }
    
    [Test(TestOf = typeof(Transformer),Author = "Cheese",Description = "Tests a function definition w/o any arguments or body")]
    public void FunctionDefinitionNoArgumentsNoBody()
    {
        const string patch = 
@"
@function test-function() {
}
";
        var validator = new PatchValidator
        {
            new FunctionValidator
            {
                Name = "test-function",
                Arguments = new(),
                Body = new()
            }
        };
        Match(patch,validator);
    }
}