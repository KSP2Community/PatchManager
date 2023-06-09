using System.Linq;
using Antlr4.Runtime.Tree;
using BepInEx.Logging;
using PatchManager.SassyPatching.Nodes;
using PatchManager.SassyPatching.Nodes.Attributes;
using PatchManager.SassyPatching.Nodes.Expressions;
using PatchManager.SassyPatching.Nodes.Expressions.Binary;
using PatchManager.SassyPatching.Nodes.Expressions.Unary;
using PatchManager.SassyPatching.Nodes.Indexers;
using PatchManager.SassyPatching.Nodes.Selectors;
using PatchManager.SassyPatching.Nodes.Statements;
using PatchManager.SassyPatching.Nodes.Statements.FunctionLevel;
using PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;
using PatchManager.SassyPatching.Nodes.Statements.TopLevel;
using SassyPatchGrammar;

namespace PatchManager.SassyPatching;

public class Transformer : sassy_parserBaseVisitor<Node>
{
    private readonly Action<string> _logError;
    public bool Errored;
    public Transformer( Action<string> logError)
    {
        _logError = logError;
    }

    private Node Error(Coordinate location, string error)
    {
        _logError($"{location.ToString()}: {error}");
        Errored = true;
        return new ErrorNode(location, error);
    }
    
    public override Node VisitPatch(sassy_parser.PatchContext context) =>
        new SassyPatch(context.GetCoordinate(),
            context.top_level_statement().Select(Visit)
                .ToList());

    public override Node VisitImport_declaration(sassy_parser.Import_declarationContext context) =>
        new Import(context.GetCoordinate(),
            context.imp
                .Text
                .Unescape());

    public override Node VisitVar_decl(sassy_parser.Var_declContext context) =>
        new VariableDeclaration(context.GetCoordinate(),
            context.variable.Text.TrimFirst(),
            Visit(context.val));

    public override Node VisitStage_def(sassy_parser.Stage_defContext context)
    {
        var location = context.GetCoordinate();
        return ulong.TryParse(context.priority.Text,
            out var priority)
            ? new StageDefinition(location,
                context.stage.Text.Unescape(),
                priority)
            : Error(location,
                "stage priority must be an unsigned integer");
    }

    public override Node VisitFunction_def(sassy_parser.Function_defContext context) =>
        new Function(context.GetCoordinate(),
            context.name.Text,
            context.args.arg_decl()
                .Select(Visit)
                .Cast<Argument>()
                .ToList(), 
            context.body.function_statement().Select(Visit)
                .ToList());

    public override Node VisitMixin_def(sassy_parser.Mixin_defContext context) =>
        new Mixin(context.GetCoordinate(),
            context.name.Text,
            context.args.arg_decl()
                .Select(Visit)
                .Cast<Argument>()
                .ToList(),
            context.body.selector_statement().Select(Visit)
                .ToList());

    public override Node VisitTop_level_conditional(sassy_parser.Top_level_conditionalContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }
        return new Conditional(context.GetCoordinate(),
            Visit(context.cond),
            context.top_level_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    public override Node VisitTop_level_else_else(sassy_parser.Top_level_else_elseContext context) =>
        new Block(context.GetCoordinate(),
            context.top_level_statement()
                .Select(Visit)
                .ToList());

    public override Node VisitTop_level_else_if(sassy_parser.Top_level_else_ifContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }
        return new Conditional(context.GetCoordinate(),
            Visit(context.cond),
            context.top_level_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    public override Node VisitSelection_block(sassy_parser.Selection_blockContext context) =>
        new SelectionBlock(context.GetCoordinate(),
            context.attributed_selector()
                .attribute()
                .Select(Visit)
                .Cast<SelectorAttribute>()
                .ToList(),
            Visit(context.attributed_selector()
                .selector()) as Selector,
            context.selector_body()
                .selector_statement().Select(Visit)
                .ToList());

    public override Node VisitRequire_mod(sassy_parser.Require_modContext context) =>
        new RequireModAttribute(context.GetCoordinate(), context.guid.Text.Unescape());

    public override Node VisitRequire_not_mod(sassy_parser.Require_not_modContext context) =>
        new RequireNotModAttribute(context.GetCoordinate(), context.guid.Text.Unescape());

    public override Node VisitRun_at_stage(sassy_parser.Run_at_stageContext context) =>
        new RunAtStageAttribute(context.GetCoordinate(), context.stage.Text.Unescape());

    public override Node VisitSel_element(sassy_parser.Sel_elementContext context)
        => new ElementSelector(context.GetCoordinate(), context.ELEMENT().GetText());

    public override Node VisitSel_ruleset(sassy_parser.Sel_rulesetContext context)
        => new RulesetSelector(context.GetCoordinate(), context.RULESET().GetText().TrimFirst());

    public override Node VisitSel_child(sassy_parser.Sel_childContext context)
        => new ChildSelector(context.GetCoordinate(), Visit(context.parent) as Selector,
            Visit(context.child) as Selector);

    public override Node VisitSel_add_element(sassy_parser.Sel_add_elementContext context)
        => new ElementAdditionSelector(context.GetCoordinate(), context.ELEMENT().GetText().TrimFirst());

    public override Node VisitSel_class(sassy_parser.Sel_classContext context)
        => new ClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    public override Node VisitSel_name(sassy_parser.Sel_nameContext context)
        => new NameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());

    public override Node VisitSel_intersection(sassy_parser.Sel_intersectionContext context)
        => new IntersectionSelector(context.GetCoordinate(), Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    public override Node VisitSel_everything(sassy_parser.Sel_everythingContext context)
        => new WildcardSelector(context.GetCoordinate());

    public override Node VisitSel_without_class(sassy_parser.Sel_without_classContext context)
        => new WithoutClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    public override Node VisitSel_without_name(sassy_parser.Sel_without_nameContext context)
        => new WithoutNameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());

    public override Node VisitSel_combination(sassy_parser.Sel_combinationContext context)
        => new CombinationSelector(context.GetCoordinate(),Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    public override Node VisitRuleset_selector(sassy_parser.Ruleset_selectorContext context)
        => new RulesetSelector(context.GetCoordinate(), context.RULESET().GetText().TrimFirst());

    public override Node VisitCombination_selector(sassy_parser.Combination_selectorContext context)
        => new CombinationSelector(context.GetCoordinate(),Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    public override Node VisitWithout_name(sassy_parser.Without_nameContext context)
        => new WithoutNameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());
    
    public override Node VisitClass_selector(sassy_parser.Class_selectorContext context)
        => new ClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    public override Node VisitWithout_class(sassy_parser.Without_classContext context)
        => new WithoutClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    public override Node VisitName(sassy_parser.NameContext context)
        => new NameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());

    public override Node VisitAdd_element(sassy_parser.Add_elementContext context)
        => new ElementAdditionSelector(context.GetCoordinate(), context.ELEMENT().GetText().TrimFirst());

    public override Node VisitEverything(sassy_parser.EverythingContext context)
        => new WildcardSelector(context.GetCoordinate());

    public override Node VisitIntersection_selector(sassy_parser.Intersection_selectorContext context)
        => new IntersectionSelector(context.GetCoordinate(), Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    public override Node VisitElement(sassy_parser.ElementContext context)
        => new ElementSelector(context.GetCoordinate(), context.ELEMENT().GetText());

    public override Node VisitSel_level_conditional(sassy_parser.Sel_level_conditionalContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }
        return new Conditional(context.GetCoordinate(),
            Visit(context.cond),
            context.selector_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    public override Node VisitSel_level_else_else(sassy_parser.Sel_level_else_elseContext context) =>
        new Block(context.GetCoordinate(),
            context.selector_statement()
                .Select(Visit)
                .ToList());

    public override Node VisitSel_level_else_if(sassy_parser.Sel_level_else_ifContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }
        return new Conditional(context.GetCoordinate(),
            Visit(context.cond),
            context.selector_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    public override Node VisitSet_value(sassy_parser.Set_valueContext context)
        => new SetValue(context.GetCoordinate(),Visit(context.expr) as Expression);

    public override Node VisitDelete_value(sassy_parser.Delete_valueContext context)
        => new DeleteValue(context.GetCoordinate());

    public override Node VisitMerge_value(sassy_parser.Merge_valueContext context)
        => new MergeValue(context.GetCoordinate(), Visit(context.expr) as Expression);

    public override Node VisitElement_key_field(sassy_parser.Element_key_fieldContext context) =>
        new Field(context.GetCoordinate(),
            context.ELEMENT()
                .GetText(),
            context.indexor != null
                ? Visit(context.indexor) as Indexer
                : null,
            Visit(context.expr) as Expression);

    public override Node VisitString_key_field(sassy_parser.String_key_fieldContext context) =>
        new Field(context.GetCoordinate(),
            context.STRING()
                .GetText()
                .Unescape(),
            context.indexor != null
                ? Visit(context.indexor) as Indexer
                : null,
            Visit(context.expr) as Expression);

    public override Node VisitNumber_indexor(sassy_parser.Number_indexorContext context)
    {
        var location = context.GetCoordinate();
        return ulong.TryParse(context.num.Text,
            out var index)
            ? new NumberIndexer(location,
                index)
            : Error(location,
                "Index must be a positive integer");
    }

    public override Node VisitElement_indexor(sassy_parser.Element_indexorContext context) =>
        new ElementIndexer(context.GetCoordinate(), context.elem.Text);

    public override Node VisitClass_indexor(sassy_parser.Class_indexorContext context) =>
        new ClassIndexer(context.GetCoordinate(), context.clazz.Text.TrimFirst());

    public override Node VisitString_indexor(sassy_parser.String_indexorContext context) =>
        new StringIndexer(context.GetCoordinate(), context.elem.Text.Unescape());

    public override Node VisitImplicit_add(sassy_parser.Implicit_addContext context) =>
        new ImplicitAdd(context.GetCoordinate(), Visit(context.sub_expression()) as Expression);

    public override Node VisitImplicit_subtract(sassy_parser.Implicit_subtractContext context) =>
        new ImplicitSubtract(context.GetCoordinate(), Visit(context.sub_expression()) as Expression);

    public override Node VisitImplicit_multiply(sassy_parser.Implicit_multiplyContext context) =>
        new ImplicitMultiply(context.GetCoordinate(), Visit(context.sub_expression()) as Expression);

    public override Node VisitImplicit_divide(sassy_parser.Implicit_divideContext context) =>
        new ImplicitDivide(context.GetCoordinate(), Visit(context.sub_expression()) as Expression);

    public override Node VisitNot_equal_to(sassy_parser.Not_equal_toContext context) =>
        new NotEqualTo(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitVariable_reference(sassy_parser.Variable_referenceContext context) =>
        new VariableReference(context.GetCoordinate(),context.VARIABLE().GetText().TrimFirst());

    public override Node VisitEqual_to(sassy_parser.Equal_toContext context) =>
        new EqualTo(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitIndexor(sassy_parser.IndexorContext context) =>
        new Subscript(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitOr(sassy_parser.OrContext context) =>
        new Or(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitLesser_than_equal(sassy_parser.Lesser_than_equalContext context) =>
        new LesserThanEqual(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitSubtraction(sassy_parser.SubtractionContext context) =>
        new Subtract(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitPositive(sassy_parser.PositiveContext context) =>
        new Positive(context.GetCoordinate(), Visit(context.child) as Expression);

    public override Node VisitSimple_call(sassy_parser.Simple_callContext context) =>
        new SimpleCall(context.GetCoordinate(), context.lhs.Text,
            context.args.argument().Select(Visit).Cast<CallArgument>().ToList());

    public override Node VisitDivision(sassy_parser.DivisionContext context) =>
        new Divide(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitNegative(sassy_parser.NegativeContext context) =>
        new Negate(context.GetCoordinate(), Visit(context.child) as Expression);

    public override Node VisitNot(sassy_parser.NotContext context) =>
        new Not(context.GetCoordinate(), Visit(context.child) as Expression);

    public override Node VisitLesser_than(sassy_parser.Lesser_thanContext context) =>
        new LesserThan(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitMember_call(sassy_parser.Member_callContext context) =>
        new MemberCall(context.GetCoordinate(), Visit(context.lhs) as Expression, context.name.Text,
            context.args.children.Select(Visit).Cast<CallArgument>().ToList());

    public override Node VisitGreater_than(sassy_parser.Greater_thanContext context) =>
        new GreaterThan(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitAnd(sassy_parser.AndContext context) =>
        new And(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitMultiplication(sassy_parser.MultiplicationContext context) =>
        new Multiply(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitRemainder(sassy_parser.RemainderContext context) =>
        new Remainder(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitGreater_than_equal(sassy_parser.Greater_than_equalContext context) =>
        new GreaterThanEqual(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitTernary(sassy_parser.TernaryContext context) =>
        new Ternary(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.cond) as Expression,
            Visit(context.rhs) as Expression);

    public override Node VisitAddition(sassy_parser.AdditionContext context) =>
        new Add(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    public override Node VisitValue_deletion(sassy_parser.Value_deletionContext context)
        => new ValueNode(context.GetCoordinate(), new Value(Value.ValueType.Deletion, null));

    public override Node VisitBoolean_true(sassy_parser.Boolean_trueContext context)
        => new ValueNode(context.GetCoordinate(), true);

    public override Node VisitBoolean_false(sassy_parser.Boolean_falseContext context)
        => new ValueNode(context.GetCoordinate(), false);

    public override Node VisitNumber_value(sassy_parser.Number_valueContext context)
    {
        var location = context.GetCoordinate();
        if (double.TryParse(context.NUMBER().GetText(), out var dbl))
        {
            return new ValueNode(location, dbl);
        }
        return Error(location, "Numbers must be parsable as a double precision floating point number");
    }

    public override Node VisitString_value(sassy_parser.String_valueContext context)
        => new ValueNode(context.GetCoordinate(), context.STRING().GetText().Unescape());

    public override Node VisitNone(sassy_parser.NoneContext context)
        => new ValueNode(context.GetCoordinate(), new Value(Value.ValueType.None, null));

    public override Node VisitList_value(sassy_parser.List_valueContext context)
        => new ListNode(context.GetCoordinate(),
            context.list().list_values().sub_expression().Select(Visit).Cast<Expression>().ToList());

    public override Node VisitObject_value(sassy_parser.Object_valueContext context)
        => new ObjectNode(context.GetCoordinate(),
            context.obj().obj_values().key_value().Select(Visit).Cast<KeyValueNode>().ToList());

    public override Node VisitLiteral_key(sassy_parser.Literal_keyContext context)
        => new KeyValueNode(context.GetCoordinate(), context.key.Text, Visit(context.val) as Expression);

    public override Node VisitString_key(sassy_parser.String_keyContext context)
        => new KeyValueNode(context.GetCoordinate(), context.key.Text.Unescape(), Visit(context.val) as Expression);

    public override Node VisitNamed_argument(sassy_parser.Named_argumentContext context)
        => new CallArgument(context.GetCoordinate(), context.key.Text.TrimFirst(), Visit(context.val) as Expression);

    public override Node VisitUnnamed_argument(sassy_parser.Unnamed_argumentContext context)
        => new CallArgument(context.GetCoordinate(), Visit(context.val) as Expression);

    public override Node VisitFn_level_conditional(sassy_parser.Fn_level_conditionalContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }
        return new Conditional(context.GetCoordinate(),
            Visit(context.cond),
            context.function_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }


    public override Node VisitFn_level_else_else(sassy_parser.Fn_level_else_elseContext context) =>
        new Block(context.GetCoordinate(),
            context.function_statement()
                .Select(Visit)
                .ToList());

    public override Node VisitFn_level_else_if(sassy_parser.Fn_level_else_ifContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }
        return new Conditional(context.GetCoordinate(),
            Visit(context.cond),
            context.function_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    public override Node VisitFn_return(sassy_parser.Fn_returnContext context)
    {
        return new Return(context.GetCoordinate(), Visit(context.sub_expression()) as Expression);
    }

    public override Node VisitMixin_include(sassy_parser.Mixin_includeContext context) =>
        new MixinInclude(context.GetCoordinate(), context.mixin.Text,
        context.args.argument().Select(Visit).Cast<CallArgument>().ToList());

    public override Node VisitArgument_without_default(sassy_parser.Argument_without_defaultContext context)
        => new Argument(context.GetCoordinate(), context.name.Text.TrimFirst());

    public override Node VisitArgument_with_default(sassy_parser.Argument_with_defaultContext context)
        => new Argument(context.GetCoordinate(), context.name.Text.TrimFirst(),Visit(context.val));
}