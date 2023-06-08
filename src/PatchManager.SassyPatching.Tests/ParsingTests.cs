using PatchManager.SassyPatching.Tests.Validators;
using PatchManager.SassyPatching.Tests.Validators.Attributes;
using PatchManager.SassyPatching.Tests.Validators.Expressions;
using PatchManager.SassyPatching.Tests.Validators.Indexers;
using PatchManager.SassyPatching.Tests.Validators.Selectors;
using PatchManager.SassyPatching.Tests.Validators.Statements;
using PatchManager.SassyPatching.Tests.Validators.Statements.FunctionLevel;
using PatchManager.SassyPatching.Tests.Validators.Statements.SelectionLevel;
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
        Assert.That(validator.Validate(parsed), Is.True,"Parse tree does not match the validation criterion");
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

    #region Selector Tests

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

    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a selector")]
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

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a selection level variable declaration")]
    public void SelectionLevelVariableDeclaration()
    {
        const string patch =
            @"
* {
    $variable: 5
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
    }

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
                }
            }
        };
        Match(patch, validator);
    }

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
                }
            }
        };
        Match(patch, validator);
    }

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
                }
            }
        };
        Match(patch, validator);
    }

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
                }
            }
        };
        Match(patch, validator);
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a simple set value selection action")]
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
    
    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a simple deletion selection action")]
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
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a simple merge selection action")]
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

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a simple field set selection action w/ an element key and no indexer")]
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
                Attributes = new (),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        FieldValue = new ValueValidator
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
        Description = "Tests a simple field set selection action w/ a string key and no indexer")]
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
                Attributes = new (),
                Selector = new WildcardSelectorValidator(),
                Actions = new()
                {
                    new FieldValidator
                    {
                        FieldName = "x",
                        FieldValue = new ValueValidator
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
        Description = "Tests a simple field set selection action w/ an element key and a number indexer")]
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
                Attributes = new (),
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
                            StoredValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a simple field set selection action w/ a string key and a number indexer")]
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
                Attributes = new (),
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
                            StoredValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a simple field set selection action w/ an element key and an element indexer")]
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
                Attributes = new (),
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
                            StoredValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a simple field set selection action w/ a string key and an element indexer")]
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
                Attributes = new (),
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
                            StoredValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a simple field set selection action w/ an element key and a class indexer")]
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
                Attributes = new (),
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
                            StoredValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a simple field set selection action w/ a string key and a class indexer")]
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
                Attributes = new (),
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
                            StoredValue = 5
                        }
                    }
                }
            }
        };
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a simple field set selection action w/ an element key and a string indexer")]
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
                Attributes = new (),
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
                            StoredValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

    [Test(TestOf = typeof(Transformer), Author = "Cheese",
        Description = "Tests a simple field set selection action w/ a string key and a string indexer")]
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
                Attributes = new (),
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
                            StoredValue = 5
                        }
                    }
                }
            }
        };
        Match(patch, validator);
    }

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

    [Test(TestOf = typeof(Transformer), Author = "Cheese", Description = "Tests a mixin include as a selection action")]
    public void MixinInclude()
    {
        const string patch =
            @"
* {
    @include test-mixin();
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
}