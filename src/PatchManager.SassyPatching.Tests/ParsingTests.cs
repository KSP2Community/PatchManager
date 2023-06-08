using PatchManager.SassyPatching.Tests.Validators;
using PatchManager.SassyPatching.Tests.Validators.Attributes;
using PatchManager.SassyPatching.Tests.Validators.Expressions;
using PatchManager.SassyPatching.Tests.Validators.Selectors;
using PatchManager.SassyPatching.Tests.Validators.Statements;
using PatchManager.SassyPatching.Tests.Validators.Statements.FunctionLevel;
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
        var parsed = Parse(patch);
        Assert.That(validator.Validate(parsed), Is.True);
    }

    #region Top Level Statement Tests

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
        Match(patch, validator);
    }


    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a top level variable declaration")]
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
        Match(patch, validator);
    }

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
                            StoredValue = 5
                        }
                    }
                },
                Body = new()
            }
        };
        Match(patch, validator);
    }

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
                            StoredValue = 5
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

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a function definition w/o any arguments or body")]
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

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a function definition w/ a single argument that has no default value and no body")]
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

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a function definition w/ a single argument that has a default value and no body")]
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
                            StoredValue = 5
                        }
                    }
                },
                Body = new()
            }
        };
        Match(patch, validator);
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description =
            "Tests a function definition w/ a single argument that has a default value and a body that returns that value")]
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
                            StoredValue = 5
                        }
                    }
                },
                Body = new()
                {
                    new VarDeclValidator()
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
                    StoredValue = true
                },
                Body = new()
                {
                    new VarDeclValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

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
                    StoredValue = true
                },
                Body = new()
                {
                    new VarDeclValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredValue = 5
                        }
                    }
                },
                Else = new BlockValidator
                {
                    new VarDeclValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredValue = 6
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

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
                    StoredValue = true
                },
                Body = new()
                {
                    new VarDeclValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredValue = 5
                        }
                    }
                },
                Else = new ConditionalValidator
                {
                    Condition = new ValueValidator
                    {
                        StoredValue = false
                    },
                    Body = new()
                    {
                        new VarDeclValidator
                        {
                            Variable = "variable",
                            Value = new ValueValidator
                            {
                                StoredValue = 6
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

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
                    StoredValue = true
                },
                Body = new()
                {
                    new VarDeclValidator
                    {
                        Variable = "variable",
                        Value = new ValueValidator
                        {
                            StoredValue = 5
                        }
                    }
                },
                Else = new ConditionalValidator
                {
                    Condition = new ValueValidator
                    {
                        StoredValue = false
                    },
                    Body = new()
                    {
                        new VarDeclValidator
                        {
                            Variable = "variable",
                            Value = new ValueValidator
                            {
                                StoredValue = 6
                            }
                        }
                    },
                    Else = new BlockValidator
                    {
                        new VarDeclValidator
                        {
                            Variable = "variable",
                            Value = new ValueValidator
                            {
                                StoredValue = 7
                            }
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

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
}