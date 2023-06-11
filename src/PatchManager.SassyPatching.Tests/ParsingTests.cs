using PatchManager.SassyPatching.Tests.Validators;
using PatchManager.SassyPatching.Tests.Validators.Attributes;
using PatchManager.SassyPatching.Tests.Validators.Expressions;
using PatchManager.SassyPatching.Tests.Validators.Indexers;
using PatchManager.SassyPatching.Tests.Validators.Selectors;
using PatchManager.SassyPatching.Tests.Validators.Statements;
using PatchManager.SassyPatching.Tests.Validators.Statements.FunctionLevel;
using PatchManager.SassyPatching.Tests.Validators.Statements.SelectionLevel;
using PatchManager.SassyPatching.Tests.Validators.Statements.TopLevel;
// ReSharper disable RedundantEmptyObjectOrCollectionInitializer

namespace PatchManager.SassyPatching.Tests;

/// <summary>
/// A class containing all of the tests for the parser and the Transformer class
/// </summary>
public class ParsingTests
{
#pragma warning disable CS8618
    private Transformer _tokenTransformer;
#pragma warning restore CS8618

    /// <summary>
    /// Sets up the parser tests by creating the transformer to be tested
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _tokenTransformer = new Transformer(Assert.Fail);
    }

    private class FailErrorListener : IAntlrErrorListener<IToken>
    {
        internal static readonly FailErrorListener Instance = new();
        private FailErrorListener() {}
        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
            string msg, RecognitionException e)
        {
            Assert.Fail($"{line}:{charPositionInLine}: {msg}");
        }
    }

    private SassyPatch Parse(string testPatch)
    {
        var charStream = CharStreams.fromString(testPatch);
        var lexer = new sassy_lexer(charStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new sassy_parser(tokenStream);
        parser.AddErrorListener(FailErrorListener.Instance);
        var patchContext = parser.patch();
        _tokenTransformer.Errored = false;
        var patch = _tokenTransformer.Visit(patchContext) as SassyPatch;
        Assert.That(_tokenTransformer.Errored, Is.False);
        return patch!;
    }

    private void Match(string patch, ParseValidator validator)
    {
        var parsed = Parse(patch);
        Assert.That(validator.Validate(parsed), Is.True, "Parse tree does not match the validation criterion");
    }

    #region Top Level Statement Tests

    /// <summary>
    /// Tests single line comments
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests single line comments")]
    public void SingleLineComments()
    {
        const string patch =
            @"
// This is a single line comment
$variable: 5;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "variable",
                Value = new ValueValidator
                {
                    StoredDataValue = 5
                }
            }
        };

        Match(patch, validator);
    }

    /// <summary>
    /// Tests single line comments
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests multi line comments")]
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
            new VariableDeclarationValidator
            {
                Variable = "variable",
                Value = new ValueValidator
                {
                    StoredDataValue = 5
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an import declaration
    /// </summary>
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
        Match(patch, validator);
    }


    /// <summary>
    /// Tests a top level variable declaration
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a top level variable declaration")]
    public void TopLevelVariableDeclaration()
    {
        const string patch =
            @"
$variable: 5;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "variable",
                Value = new ValueValidator
                {
                    StoredDataValue = 5
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a stage definition
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a stage definition")]
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
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a function definition w/o any arguments or body
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a function definition w/o any arguments or body")]
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
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a function definition w/ a single argument that has no default value and no body
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a function definition w/ a single argument that has no default value and no body")]
    public void FunctionDefinitionSingleArgumentNoDefaultNoBody()
    {
        const string patch =
            @"
@function test-function($arg) {
}
";
        var validator = new PatchValidator
        {
            new FunctionValidator
            {
                Name = "test-function",
                Arguments = new()
                {
                    new ArgumentValidator
                    {
                        Name = "arg"
                    }
                },
                Body = new()
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a function definition w/ a single argument that has a default value and no body
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a function definition w/ a single argument that has a default value and no body")]
    public void FunctionDefinitionSingleArgumentDefaultNoBody()
    {
        const string patch =
            @"
@function test-function($arg: 5) {
}
";
        var validator = new PatchValidator
        {
            new FunctionValidator
            {
                Name = "test-function",
                Arguments = new()
                {
                    new ArgumentValidator
                    {
                        Name = "arg",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                },
                Body = new()
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a function definition w/ a single argument that has a default value and a body that returns that value
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description =
            "Tests a function definition w/ a single argument that has a default value and a body that returns that value")]
    public void FunctionDefinitionSingleArgumentDefaultReturn()
    {
        const string patch =
            @"
@function test-function($arg: 5) {
    @return $arg;
}
";
        var validator = new PatchValidator
        {
            new FunctionValidator
            {
                Name = "test-function",
                Arguments = new()
                {
                    new ArgumentValidator
                    {
                        Name = "arg",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                },
                Body = new()
                {
                    new ReturnValidator
                    {
                        ReturnedValue = new VariableReferenceValidator
                        {
                            VariableName = "arg"
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a mixin definition w/o any arguments or body
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a mixin definition w/o any arguments or body")]
    public void MixinDefinitionNoArgumentsNoBody()
    {
        const string patch =
            @"
@mixin test-mixin() {
}
";
        var validator = new PatchValidator
        {
            new MixinValidator
            {
                Name = "test-mixin",
                Arguments = new(),
                Body = new()
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a mixin definition w/ a single argument that has no default value and no body
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a mixin definition w/ a single argument that has no default value and no body")]
    public void MixinDefinitionSingleArgumentNoDefaultNoBody()
    {
        const string patch =
            @"
@mixin test-mixin($arg) {
}
";
        var validator = new PatchValidator
        {
            new MixinValidator
            {
                Name = "test-mixin",
                Arguments = new()
                {
                    new ArgumentValidator
                    {
                        Name = "arg"
                    }
                },
                Body = new()
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a mixin definition w/ a single argument that has a default value and no body
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a mixin definition w/ a single argument that has a default value and no body")]
    public void MixinDefinitionSingleArgumentDefaultNoBody()
    {
        const string patch =
            @"
@mixin test-mixin($arg: 5) {
}
";
        var validator = new PatchValidator
        {
            new MixinValidator
            {
                Name = "test-mixin",
                Arguments = new()
                {
                    new ArgumentValidator
                    {
                        Name = "arg",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                },
                Body = new()
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a mixin definition w/ a single argument that has a default value and a body that returns that value
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description =
            "Tests a mixin definition w/ a single argument that has a default value and a body that returns that value")]
    public void MixinDefinitionSingleArgumentDefaultBody()
    {
        const string patch =
            @"
@mixin test-mixin($arg: 5) {
    $var: $arg;
}
";
        var validator = new PatchValidator
        {
            new MixinValidator
            {
                Name = "test-mixin",
                Arguments = new()
                {
                    new ArgumentValidator
                    {
                        Name = "arg",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                },
                Body = new()
                {
                    new VariableDeclarationValidator()
                    {
                        Variable = "var",
                        Value = new VariableReferenceValidator
                        {
                            VariableName = "arg"
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a top level conditional statement w/ no else or else if
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a top level conditional statement w/ no else or else if")]
    public void TopLevelConditionalNoElseIfNoElse()
    {
        const string patch =
            @"
@if true {
    $variable: 5;
}
";
        var validator = new PatchValidator
        {
            new ConditionalValidator
            {
                Condition = new ValueValidator
                {
                    StoredDataValue = true
                },
                Body = new()
                {
                    new VariableDeclarationValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a top level conditional statement w/ an else but no else if
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a top level conditional statement w/ an else but no else if")]
    public void TopLevelConditionalNoElseIfElse()
    {
        const string patch =
            @"
@if true 
{
    $variable: 5;
} 
@else 
{
    $variable: 6;
}
";
        var validator = new PatchValidator
        {
            new ConditionalValidator
            {
                Condition = new ValueValidator
                {
                    StoredDataValue = true
                },
                Body = new()
                {
                    new VariableDeclarationValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                },
                Else = new BlockValidator
                {
                    new VariableDeclarationValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 6
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a top level conditional statement w/ an else if but no else
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a top level conditional statement w/ an else if but no else")]
    public void TopLevelConditionalElseIfNoElse()
    {
        const string patch =
            @"
@if true 
{
    $variable: 5;
} 
@else-if false
{
    $variable: 6;
}
";
        var validator = new PatchValidator
        {
            new ConditionalValidator
            {
                Condition = new ValueValidator
                {
                    StoredDataValue = true
                },
                Body = new()
                {
                    new VariableDeclarationValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                },
                Else = new ConditionalValidator
                {
                    Condition = new ValueValidator
                    {
                        StoredDataValue = false
                    },
                    Body = new()
                    {
                        new VariableDeclarationValidator
                        {
                            Variable = "variable",
                            Value = new ValueValidator
                            {
                                StoredDataValue = 6
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a top level conditional statement w/ an else if and an else
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a top level conditional statement w/ an else if and an else")]
    public void TopLevelConditionalElseIfElse()
    {
        const string patch =
            @"
@if true 
{
    $variable: 5;
} 
@else-if false
{
    $variable: 6;
}
@else
{
    $variable: 7;
}
";
        var validator = new PatchValidator
        {
            new ConditionalValidator
            {
                Condition = new ValueValidator
                {
                    StoredDataValue = true
                },
                Body = new()
                {
                    new VariableDeclarationValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                },
                Else = new ConditionalValidator
                {
                    Condition = new ValueValidator
                    {
                        StoredDataValue = false
                    },
                    Body = new()
                    {
                        new VariableDeclarationValidator
                        {
                            Variable = "variable",
                            Value = new ValueValidator
                            {
                                StoredDataValue = 6
                            }
                        }
                    },
                    Else = new BlockValidator
                    {
                        new VariableDeclarationValidator
                        {
                            Variable = "variable",
                            Value = new ValueValidator
                            {
                                StoredDataValue = 7
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a simple selection block
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a simple selection block")]
    public void SelectionBlock()
    {
        const string patch =
            @"
* {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
            }
        };
        Match(patch, validator);
    }

    #endregion

    #region Attribute Tests

    /// <summary>
    /// Tests a selection block w/ an @require attribute
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a selection block w/ an @require attribute")]
    public void RequireAttribute()
    {
        const string patch =
            @"
@require 'mod'
* {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new()
                {
                    new RequireModAttributeValidator
                    {
                        Guid = "mod"
                    }
                },
                Selector = new WildcardSelectorValidator(),
                Actions = new()
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a selection block w/ an @require-not attribute
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a selection block w/ an @require-not attribute")]
    public void RequireNotAttribute()
    {
        const string patch =
            @"
@require-not 'mod'
* {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new()
                {
                    new RequireNotModAttributeValidator
                    {
                        Guid = "mod"
                    }
                },
                Selector = new WildcardSelectorValidator(),
                Actions = new()
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a selection block w/ an @stage attribute
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a selection block w/ an @stage attribute")]
    public void StageAttribute()
    {
        const string patch =
            @"
@stage 'init'
* {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new()
                {
                    new RunAtStageAttributeValidator()
                    {
                        Stage = "init"
                    }
                },
                Selector = new WildcardSelectorValidator(),
                Actions = new()
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a selection block w/ a @require, a @require-not, and a @stage attribute
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a selection block w/ a @require, a @require-not, and a @stage attribute")]
    public void MultipleAttributes()
    {
        const string patch =
            @"
@require 'mod'
@require-not 'mod'
@stage 'init'
* {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new()
                {
                    new RequireModAttributeValidator
                    {
                        Guid = "mod"
                    },
                    new RequireNotModAttributeValidator
                    {
                        Guid = "mod"
                    },
                    new RunAtStageAttributeValidator()
                    {
                        Stage = "init"
                    }
                },
                Selector = new WildcardSelectorValidator(),
                Actions = new()
            }
        };
        Match(patch, validator);
    }

    #endregion

    #region Selector Tests

    /// <summary>
    /// Tests an element selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an element selector")]
    public void ElementSelector()
    {
        const string patch =
            @"
element {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new ElementSelectorValidator
                {
                    ElementName = "element"
                },
                Actions = new(),
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a class selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a class selector")]
    public void ClassSelector()
    {
        const string patch =
            @"
.class {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new ClassSelectorValidator
                {
                    ClassName = "class"
                },
                Actions = new(),
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a name selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a name selector")]
    public void NameSelector()
    {
        const string patch =
            @"
#name {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new NameSelectorValidator
                {
                    NamePattern = "name"
                },
                Actions = new(),
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a ruleset selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a ruleset selector")]
    public void RulesetSelector()
    {
        const string patch =
            @"
:ruleset {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new RulesetSelectorValidator
                {
                    RulesetName = "ruleset"
                },
                Actions = new(),
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a combination selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a combination selector")]
    public void CombinationSelector()
    {
        const string patch =
            @"
a, b, c {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new CombinationSelectorValidator
                {
                    new ElementSelectorValidator
                    {
                        ElementName = "a"
                    },
                    new ElementSelectorValidator
                    {
                        ElementName = "b"
                    },
                    new ElementSelectorValidator
                    {
                        ElementName = "c"
                    }
                },
                Actions = new(),
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a child selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a child selector")]
    public void ChildSelector()
    {
        const string patch =
            @"
a > .b {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new ChildSelectorValidator
                {
                    Parent = new ElementSelectorValidator
                    {
                        ElementName = "a"
                    },
                    Child = new ClassSelectorValidator
                    {
                        ClassName = "b"
                    }
                },
                Actions = new(),
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an intersection selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an intersection selector")]
    public void IntersectionSelector()
    {
        const string patch =
            @"
a b c {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new IntersectionSelectorValidator
                {
                    new ElementSelectorValidator
                    {
                        ElementName = "a"
                    },
                    new ElementSelectorValidator
                    {
                        ElementName = "b"
                    },
                    new ElementSelectorValidator
                    {
                        ElementName = "c"
                    }
                },
                Actions = new(),
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a without class selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a without class selector")]
    public void WithoutClassSelector()
    {
        const string patch =
            @"
~.class {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WithoutClassSelectorValidator
                {
                    ClassName = "class"
                },
                Actions = new(),
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a without name selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a without name selector")]
    public void WithoutNameSelector()
    {
        const string patch =
            @"
~#name {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WithoutNameSelectorValidator
                {
                    NamePattern = "name"
                },
                Actions = new(),
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a wildcard selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a wildcard selector")]
    public void WildcardSelector()
    {
        const string patch =
            @"
* {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a complex child selector
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a complex child selector")]
    public void ComplexChildSelector()
    {
        const string patch =
            @"
:parts * > ResourceContainers > * {
    // Empty
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new ChildSelectorValidator
                {
                    Parent = new ChildSelectorValidator
                    {
                        Parent = new IntersectionSelectorValidator
                        {
                            new RulesetSelectorValidator
                            {
                                RulesetName = "parts"
                            },
                            new WildcardSelectorValidator()
                        },
                        Child = new ElementSelectorValidator
                        {
                            ElementName = "ResourceContainers"
                        }
                    },
                    Child = new WildcardSelectorValidator()
                },
                Actions = new(),
            }
        };
        Match(patch, validator);
    }

    #endregion

    #region Selection Action Tests

    /// <summary>
    /// Tests a selection level variable declaration
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a selection level variable declaration")]
    public void SelectionLevelVariableDeclaration()
    {
        const string patch =
            @"
* {
    $variable: 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new VariableDeclarationValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a selection level conditional statement w/ no else or else if
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a selection level conditional statement w/ no else or else if")]
    public void SelectionLevelConditionalNoElseIfNoElse()
    {
        const string patch =
            @"
* {
    @if true {
        $variable: 5;
    }
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new ConditionalValidator
                    {
                        Condition = new ValueValidator
                        {
                            StoredDataValue = true
                        },
                        Body = new()
                        {
                            new VariableDeclarationValidator
                            {
                                Variable = "variable",
                                Value = new ValueValidator
                                {
                                    StoredDataValue = 5
                                }
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a selection level conditional statement w/ an else but no else if
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a selection level conditional statement w/ an else but no else if")]
    public void SelectionLevelConditionalNoElseIfElse()
    {
        const string patch =
            @"
* {
    @if true 
    {
        $variable: 5;
    } 
    @else 
    {
        $variable: 6;
    }
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new ConditionalValidator
                    {
                        Condition = new ValueValidator
                        {
                            StoredDataValue = true
                        },
                        Body = new()
                        {
                            new VariableDeclarationValidator
                            {
                                Variable = "variable",
                                Value = new ValueValidator
                                {
                                    StoredDataValue = 5
                                }
                            }
                        },
                        Else = new BlockValidator
                        {
                            new VariableDeclarationValidator
                            {
                                Variable = "variable",
                                Value = new ValueValidator
                                {
                                    StoredDataValue = 6
                                }
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a selection level conditional statement w/ an else if but no else
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a selection level conditional statement w/ an else if but no else")]
    public void SelectionLevelConditionalElseIfNoElse()
    {
        const string patch =
            @"
* {
    @if true 
    {
        $variable: 5;
    } 
    @else-if false
    {
        $variable: 6;
    }
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new ConditionalValidator
                    {
                        Condition = new ValueValidator
                        {
                            StoredDataValue = true
                        },
                        Body = new()
                        {
                            new VariableDeclarationValidator
                            {
                                Variable = "variable",
                                Value = new ValueValidator
                                {
                                    StoredDataValue = 5
                                }
                            }
                        },
                        Else = new ConditionalValidator
                        {
                            Condition = new ValueValidator
                            {
                                StoredDataValue = false
                            },
                            Body = new()
                            {
                                new VariableDeclarationValidator
                                {
                                    Variable = "variable",
                                    Value = new ValueValidator
                                    {
                                        StoredDataValue = 6
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a selection level conditional statement w/ an else if and an else
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a selection level conditional statement w/ an else if and an else")]
    public void SelectionLevelConditionalElseIfElse()
    {
        const string patch =
            @"
* {
    @if true 
    {
        $variable: 5;
    } 
    @else-if false
    {
        $variable: 6;
    }
    @else
    {
        $variable: 7;
    }
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new ConditionalValidator
                    {
                        Condition = new ValueValidator
                        {
                            StoredDataValue = true
                        },
                        Body = new()
                        {
                            new VariableDeclarationValidator
                            {
                                Variable = "variable",
                                Value = new ValueValidator
                                {
                                    StoredDataValue = 5
                                }
                            }
                        },
                        Else = new ConditionalValidator
                        {
                            Condition = new ValueValidator
                            {
                                StoredDataValue = false
                            },
                            Body = new()
                            {
                                new VariableDeclarationValidator
                                {
                                    Variable = "variable",
                                    Value = new ValueValidator
                                    {
                                        StoredDataValue = 6
                                    }
                                }
                            },
                            Else = new BlockValidator
                            {
                                new VariableDeclarationValidator
                                {
                                    Variable = "variable",
                                    Value = new ValueValidator
                                    {
                                        StoredDataValue = 7
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a set value selection action
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a set value selection action")]
    public void SetValueAction()
    {
        const string patch =
            @"
// Note, don't ever run this unless you want to not have a game
* {
    @set {};
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new SetValueValidator
                    {
                        Value = new ObjectValidator
                        {
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a deletion selection action
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a deletion selection action")]
    public void DeletionAction()
    {
        const string patch =
            @"
// Note, don't ever run this unless you want to not have a game
* {
    @delete;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new DeleteValueValidator()
                }
            }
        };
        Match(patch,validator);
    }

    /// <summary>
    /// Tests a merge selection action
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a merge selection action")]
    public void MergeAction()
    {
        const string patch =
            @"
* {
    @merge {};
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new MergeValueValidator
                    {
                        Value = new ObjectValidator
                        {
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a field set selection action w/ an element key and no indexer
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a field set selection action w/ an element key and no indexer")]
    public void FieldSetElementKeyNoIndexer()
    {
        const string patch =
            @"
* {
    x: 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        FieldValue = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a field set selection action w/ a string key and no indexer
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a field set selection action w/ a string key and no indexer")]
    public void FieldSetStringKeyNoIndexer()
    {
        const string patch =
            @"
* {
    'x': 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        FieldValue = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a field set selection action w/ an element key and a number indexer
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a field set selection action w/ an element key and a number indexer")]
    public void FieldSetElementKeyNumberIndexer()
    {
        const string patch =
            @"
* {
    x[0]: 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        Indexer = new NumberIndexerValidator
                        {
                            Index = 0
                        },
                        FieldValue = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a field set selection action w/ a string key and a number indexer
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a field set selection action w/ a string key and a number indexer")]
    public void FieldSetStringKeyNumberIndexer()
    {
        const string patch =
            @"
* {
    'x'[0]: 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        Indexer = new NumberIndexerValidator
                        {
                            Index = 0
                        },
                        FieldValue = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a field set selection action w/ an element key and an element indexer
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a field set selection action w/ an element key and an element indexer")]
    public void FieldSetElementKeyElementIndexer()
    {
        const string patch =
            @"
* {
    x[y]: 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        Indexer = new ElementIndexerValidator
                        {
                            ElementName = "y"
                        },
                        FieldValue = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a field set selection action w/ a string key and an element indexer
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a field set selection action w/ a string key and an element indexer")]
    public void FieldSetStringKeyElementIndexer()
    {
        const string patch =
            @"
* {
    'x'[y]: 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        Indexer = new ElementIndexerValidator
                        {
                            ElementName = "y"
                        },
                        FieldValue = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a field set selection action w/ an element key and a class indexer
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a field set selection action w/ an element key and a class indexer")]
    public void FieldSetElementKeyClassIndexer()
    {
        const string patch =
            @"
* {
    x[.y]: 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        Indexer = new ClassIndexerValidator
                        {
                            ClassName = "y"
                        },
                        FieldValue = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a field set selection action w/ a string key and a class indexer
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a field set selection action w/ a string key and a class indexer")]
    public void FieldSetStringKeyClassIndexer()
    {
        const string patch =
            @"
* {
    'x'[.y]: 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        Indexer = new ClassIndexerValidator
                        {
                            ClassName = "y"
                        },
                        FieldValue = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch,validator);
    }

    /// <summary>
    /// Tests a field set selection action w/ an element key and a string indexer
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a field set selection action w/ an element key and a string indexer")]
    public void FieldSetElementKeyStringIndexer()
    {
        const string patch =
            @"
* {
    x['y']: 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        Indexer = new StringIndexerValidator
                        {
                            Index = "y"
                        },
                        FieldValue = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a field set selection action w/ a string key and a string indexer
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a field set selection action w/ a string key and a string indexer")]
    public void FieldSetStringKeyStringIndexer()
    {
        const string patch =
            @"
* {
    'x'['y']: 5;
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        Indexer = new StringIndexerValidator
                        {
                            Index = "y"
                        },
                        FieldValue = new ValueValidator
                        {
                            StoredDataValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a nested selection block as a selection action
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a nested selection block as a selection action")]
    public void NestedSelectionBlock()
    {
        const string patch =
            @"
* {
    * {
    }
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new SelectionBlockValidator
                    {
                        Attributes = new(),
                        Selector = new WildcardSelectorValidator(),
                        Actions = new()
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a mixin include as a selection action
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a mixin include as a selection action")]
    public void MixinInclude()
    {
        const string patch =
            @"
* {
    @include test-mixin()
}
";
        var validator = new PatchValidator
        {
            new SelectionBlockValidator
            {
                Attributes = new(),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new MixinIncludeValidator
                    {
                        MixinName = "test-mixin",
                        Arguments = new()
                    }
                }
            }
        };
        Match(patch, validator);
    }

    #endregion

    #region Expression Tests

    /// <summary>
    /// Tests an implicit addition expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an implicit addition expression")]
    public void ImplicitAddition()
    {
        const string patch =
            @"
$x: +2;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new UnaryValidator<ImplicitAdd>
                {
                    Child = new ValueValidator
                    {
                        StoredDataValue = 2
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an implicit subtraction expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an implicit subtraction expression")]
    public void ImplicitSubtraction()
    {
        const string patch =
            @"
$x: -2;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new UnaryValidator<ImplicitSubtract>
                {
                    Child = new ValueValidator
                    {
                        StoredDataValue = 2
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an implicit multiplication expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an implicit multiplication expression")]
    public void ImplicitMultiplication()
    {
        const string patch =
            @"
$x: *2;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new UnaryValidator<ImplicitMultiply>
                {
                    Child = new ValueValidator
                    {
                        StoredDataValue = 2
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an implicit division expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an implicit division expression")]
    public void ImplicitDivision()
    {
        const string patch =
            @"
$x: /2;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new UnaryValidator<ImplicitDivide>
                {
                    Child = new ValueValidator
                    {
                        StoredDataValue = 2
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a variable reference expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a variable reference expression")]
    public void VariableReference()
    {
        const string patch =
            @"
$x: $y;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new VariableReferenceValidator
                {
                    VariableName = "y"
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a negation expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a negation expression")]
    public void Negate()
    {
        const string patch =
            @"
$x: (-2);
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new UnaryValidator<Negate>
                {
                    Child = new ValueValidator
                    {
                        StoredDataValue = 2
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a positive expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a positive expression")]
    public void Positive()
    {
        const string patch =
            @"
$x: (+2);
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new UnaryValidator<Positive>
                {
                    Child = new ValueValidator
                    {
                        StoredDataValue = 2
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an inversion expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an inversion expression")]
    public void Inversion()
    {
        const string patch =
            @"
$x: !false;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new UnaryValidator<Not>
                {
                    Child = new ValueValidator
                    {
                        StoredDataValue = false
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a function call w/ no unnamed or named arguments
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a function call w/ no unnamed or named arguments")]
    public void CallNoUnnamedNoNamed()
    {
        const string patch =
            @"
$x: test-function();
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new SimpleCallValidator
                {
                    FunctionName = "test-function",
                    Arguments = new()
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a function call w/ an unnamed argument but no named arguments
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a function call w/ an unnamed argument but no named arguments")]
    public void CallUnnamedNoNamed()
    {
        const string patch =
            @"
$x: test-function(5);
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new SimpleCallValidator
                {
                    FunctionName = "test-function",
                    Arguments = new()
                    {
                        new CallArgumentValidator
                        {
                            ArgumentValue = new ValueValidator
                            {
                                StoredDataValue = 5
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a function call w/ a named argument but no unnamed arguments
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a function call w/ a named argument but no unnamed arguments")]
    public void CallNoUnnamedNamed()
    {
        const string patch =
            @"
$x: test-function($a: 5);
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new SimpleCallValidator
                {
                    FunctionName = "test-function",
                    Arguments = new()
                    {
                        new CallArgumentValidator
                        {
                            ArgumentName = "a",
                            ArgumentValue = new ValueValidator
                            {
                                StoredDataValue = 5
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a function call w/ a named argument and an unnamed argument
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a function call w/ a named argument and an unnamed argument")]
    public void CallUnnamedNamed()
    {
        const string patch =
            @"
$x: test-function(6, $a: 5);
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new SimpleCallValidator
                {
                    FunctionName = "test-function",
                    Arguments = new()
                    {
                        new CallArgumentValidator
                        {
                            ArgumentValue = new ValueValidator
                            {
                                StoredDataValue = 6
                            }
                        },
                        new CallArgumentValidator
                        {
                            ArgumentName = "a",
                            ArgumentValue = new ValueValidator
                            {
                                StoredDataValue = 5
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a member call w/o any arguments
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a member call w/o any arguments")]
    public void MemberCall()
    {
        const string patch =
            @"
$x: $y:method();
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new MemberCallValidator
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    FunctionName = "method",
                    Arguments = new()
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a subscript expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a subscript expression")]
    public void Subscript()
    {
        const string patch =
            @"
$x: $y[$z];
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<Subscript>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a multiplication expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a multiplication expression")]
    public void Multiply()
    {
        const string patch =
            @"
$x: $y * $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<Multiply>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a division expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a division expression")]
    public void Divide()
    {
        const string patch =
            @"
$x: $y / $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<Divide>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a remainder expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a remainder expression")]
    public void Remainder()
    {
        const string patch =
            @"
$x: $y % $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<Remainder>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an addition expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an addition expression")]
    public void Add()
    {
        const string patch =
            @"
$x: $y + $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<Add>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a subtraction expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a subtraction expression")]
    public void Subtract()
    {
        const string patch =
            @"
$x: $y - $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<Subtract>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a greater than expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a greater than expression")]
    public void GreaterThan()
    {
        const string patch =
            @"
$x: $y > $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<GreaterThan>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a lesser than expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a lesser than expression")]
    public void LesserThan()
    {
        const string patch =
            @"
$x: $y < $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<LesserThan>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a greater than equal expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a greater than equal expression")]
    public void GreaterThanEqual()
    {
        const string patch =
            @"
$x: $y >= $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<GreaterThanEqual>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a lesser than equal expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a lesser than equal expression")]
    public void LesserThanEqual()
    {
        const string patch =
            @"
$x: $y <= $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<LesserThanEqual>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an equal to expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an equal to expression")]
    public void EqualTo()
    {
        const string patch =
            @"
$x: $y == $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<EqualTo>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a not equal to expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a not equal to expression")]
    public void NotEqualTo()
    {
        const string patch =
            @"
$x: $y != $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<NotEqualTo>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an and expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an and expression")]
    public void And()
    {
        const string patch =
            @"
$x: $y and $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<And>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an or expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an or expression")]
    public void Or()
    {
        const string patch =
            @"
$x: $y or $z;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<Or>
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a ternary expression
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a ternary expression")]
    public void Ternary()
    {
        const string patch =
            @"
$x: $y if $z else $w;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new TernaryValidator
                {
                    LeftHandSide = new VariableReferenceValidator
                    {
                        VariableName = "y"
                    },
                    Condition = new VariableReferenceValidator
                    {
                        VariableName = "z"
                    },
                    RightHandSide = new VariableReferenceValidator
                    {
                        VariableName = "w"
                    }
                }
            }
        };
        Match(patch, validator);
    }
    
    #endregion

    #region Operator Precedence Tests
    
    /// <summary>
    /// Tests that multiplication has a higher precedence than addition
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests that multiplication has a higher precedence than addition")]
    public void MultiplicativePrecedence()
    {
        const string patch =
            @"
$x: $y * $z + $w * $v;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<Add>
                {
                    LeftHandSide = new BinaryValidator<Multiply>
                    {
                        LeftHandSide = new VariableReferenceValidator
                        {
                            VariableName = "y"
                        },
                        RightHandSide = new VariableReferenceValidator
                        {
                            VariableName = "z"
                        }
                    },
                    RightHandSide = new BinaryValidator<Multiply>
                    {
                        LeftHandSide = new VariableReferenceValidator
                        {
                            VariableName = "w"
                        },
                        RightHandSide = new VariableReferenceValidator
                        {
                            VariableName = "v"
                        }
                    },
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests that subscription has a higher precedence than addition
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests that subscription has a higher precedence than addition")]
    public void SubscriptionPrecedence()
    {
        const string patch =
            @"
$x: $y * $z + $w[$v];
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new BinaryValidator<Add>
                {
                    LeftHandSide = new BinaryValidator<Multiply>
                    {
                        LeftHandSide = new VariableReferenceValidator
                        {
                            VariableName = "y"
                        },
                        RightHandSide = new VariableReferenceValidator
                        {
                            VariableName = "z"
                        }
                    },
                    RightHandSide = new BinaryValidator<Subscript>
                    {
                        LeftHandSide = new VariableReferenceValidator
                        {
                            VariableName = "w"
                        },
                        RightHandSide = new VariableReferenceValidator
                        {
                            VariableName = "v"
                        }
                    },
                }
            }
        };
        Match(patch, validator);
    }

    #endregion

    #region Value Tests

    /// <summary>
    /// Tests a deletion value
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a deletion value")]
    public void DeleteValue()
    {
        const string patch =
            @"
$x: @delete;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new ValueValidator
                {
                    StoredDataValue = new DataValue(DataValue.DataType.Deletion)
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a true value
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a true value")]
    public void True()
    {
        const string patch =
            @"
$x: true;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new ValueValidator
                {
                    StoredDataValue = true
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a false value
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a false value")]
    public void False()
    {
        const string patch =
            @"
$x: false;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new ValueValidator
                {
                    StoredDataValue = false
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a number value
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a number value")]
    public void Number()
    {
        const string patch =
            @"
$x: 6.75;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new ValueValidator
                {
                    StoredDataValue = 6.75
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a string value
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a string value")]
    public void String()
    {
        const string patch =
            @"
$x: 'a\nb\n';
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new ValueValidator
                {
                    StoredDataValue = "a\nb\n"
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a null value
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a null value")]
    public void None()
    {
        const string patch =
            @"
$x: null;
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new ValueValidator
                {
                    StoredDataValue = new DataValue(DataValue.DataType.None)
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests a list value
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a list value")]
    public void List()
    {
        const string patch =
            @"
$x: [1,'2',true,4,null];
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new ListValidator
                {
                    new ValueValidator
                    {
                        StoredDataValue = 1
                    },
                    new ValueValidator
                    {
                        StoredDataValue = "2"
                    },
                    new ValueValidator
                    {
                        StoredDataValue = true
                    },
                    new ValueValidator
                    {
                        StoredDataValue = 4
                    },
                    new ValueValidator
                    {
                        StoredDataValue = new DataValue(DataValue.DataType.None)
                    }
                }
            }
        };
        Match(patch, validator);
    }

    /// <summary>
    /// Tests an object value
    /// </summary>
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests an object value")]
    public void Object()
    {
        const string patch =
            @"
$x: {
    a: 1,
    'b': 2,
};
";
        var validator = new PatchValidator
        {
            new VariableDeclarationValidator
            {
                Variable = "x",
                Value = new ObjectValidator
                {
                    new KeyValueValidator
                    {
                        Key = "a",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 1
                        }
                    },
                    new KeyValueValidator
                    {
                        Key = "b",
                        Value = new ValueValidator
                        {
                            StoredDataValue = 2
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }
    
    #endregion
}