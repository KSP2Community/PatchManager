using System.Globalization;
using PatchManager.SassyPatching.Nodes;
using PatchManager.SassyPatching.Nodes.Attributes;
using PatchManager.SassyPatching.Nodes.Attributes.RequireExpressions;
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
using UnityEngine;

namespace PatchManager.SassyPatching;

/// <summary>
/// A class used for transforming an Antlr representation of a Patch to our own node tree representation
/// </summary>
public class Transformer : sassy_parserBaseVisitor<Node>
{
    private readonly Action<string> _logError;

    /// <summary>
    /// Whether the transformation encountered an error
    /// </summary>
    public bool Errored;

    /// <summary>
    /// Creates a new transformer
    /// </summary>
    /// <param name="logError">The function called when an error is encountered</param>
    public Transformer(Action<string> logError)
    {
        _logError = logError;
    }

    private Node Error(Coordinate location, string error)
    {
        _logError($"{location.ToString()}: {error}");
        Errored = true;
        return new ErrorNode(location, error);
    }

    /// <inheritdoc />
    public override Node VisitPatch(sassy_parser.PatchContext context) =>
        new SassyPatch(context.GetCoordinate(),
            context.top_level_statement().Select(Visit)
                .ToList());

    /// <inheritdoc />
    public override Node VisitPatch_declaration(sassy_parser.Patch_declarationContext context)
    {
        var labels = context.patch_list().sassy_string().Select(x => x.GetStringValue()).ToList();
        return new PatchDeclaration(context.GetCoordinate(), labels);
    }

    /// <inheritdoc />
    public override Node VisitImport_declaration(sassy_parser.Import_declarationContext context) =>
        new Import(context.GetCoordinate(),
            context.imp.GetStringValue());

    /// <inheritdoc />
    public override Node VisitNormal_var_decl(sassy_parser.Normal_var_declContext context)
        => new VariableDeclaration(context.GetCoordinate(),
            context.variable.Text.TrimFirst(),
            Visit(context.val) as Expression);

    /// <inheritdoc />
    public override Node VisitAdd_var_decl(sassy_parser.Add_var_declContext context) => new VariableDeclaration(
        context.GetCoordinate(),
        context.variable.Text.TrimFirst(),
        new ImplicitAdd(context.GetCoordinate(), Visit(context.val) as Expression));

    /// <inheritdoc />
    public override Node VisitSubtract_var_decl(sassy_parser.Subtract_var_declContext context) =>
        new VariableDeclaration(
            context.GetCoordinate(),
            context.variable.Text.TrimFirst(),
            new ImplicitSubtract(context.GetCoordinate(), Visit(context.val) as Expression));

    /// <inheritdoc />
    public override Node VisitMultiply_var_decl(sassy_parser.Multiply_var_declContext context) =>
        new VariableDeclaration(
            context.GetCoordinate(),
            context.variable.Text.TrimFirst(),
            new ImplicitMultiply(context.GetCoordinate(), Visit(context.val) as Expression));

    /// <inheritdoc />
    public override Node VisitDivide_var_decl(sassy_parser.Divide_var_declContext context) => new VariableDeclaration(
        context.GetCoordinate(),
        context.variable.Text.TrimFirst(),
        new ImplicitDivide(context.GetCoordinate(), Visit(context.val) as Expression));

    /// <inheritdoc />
    public override Node VisitFunction_def(sassy_parser.Function_defContext context) =>
        new Function(context.GetCoordinate(),
            context.name.Text,
            context.args.arg_decl()
                .Select(Visit)
                .Cast<Argument>()
                .ToList(),
            context.body.function_statement().Select(Visit)
                .ToList());

    /// <inheritdoc />
    public override Node VisitClosure(sassy_parser.ClosureContext context) =>
        new Closure(context.GetCoordinate(),
            context.args.arg_decl()
                .Select(Visit)
                .Cast<Argument>()
                .ToList(),
            context.body.function_statement().Select(Visit)
                .ToList());

    /// <inheritdoc />
    public override Node VisitMixin_def(sassy_parser.Mixin_defContext context) =>
        new Mixin(context.GetCoordinate(),
            context.name.Text,
            context.args.arg_decl()
                .Select(Visit)
                .Cast<Argument>()
                .ToList(),
            context.body.selector_statement().Select(Visit)
                .ToList());

    /// <inheritdoc />
    public override Node VisitTop_level_conditional(sassy_parser.Top_level_conditionalContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }

        return new Conditional(context.GetCoordinate(),
            Visit(context.cond) as Expression,
            context.top_level_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    /// <inheritdoc />
    public override Node VisitTop_level_else_else(sassy_parser.Top_level_else_elseContext context) =>
        new Block(context.GetCoordinate(),
            context.top_level_statement()
                .Select(Visit)
                .ToList());

    /// <inheritdoc />
    public override Node VisitTop_level_else_if(sassy_parser.Top_level_else_ifContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }

        return new Conditional(context.GetCoordinate(),
            Visit(context.cond) as Expression,
            context.top_level_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override Node VisitRequire_mod(sassy_parser.Require_modContext context) =>
        new RequireModAttribute(context.GetCoordinate(), Visit(context.expr) as RequireExpression);

    public override Node VisitRequire_not(sassy_parser.Require_notContext context) =>
        new RequireNot(context.GetCoordinate(), Visit(context.internal_expr) as RequireExpression);

    public override Node VisitRequire_and(sassy_parser.Require_andContext context) =>
        new RequireAnd(context.GetCoordinate(), Visit(context.lhs) as RequireExpression,
            Visit(context.rhs) as RequireExpression);

    public override Node VisitRequire_or(sassy_parser.Require_orContext context) =>
        new RequireOr(context.GetCoordinate(), Visit(context.lhs) as RequireExpression,
            Visit(context.rhs) as RequireExpression);

    public override Node VisitRequire_guid(sassy_parser.Require_guidContext context) =>
        new RequireGuid(context.GetCoordinate(), context.modid.GetStringValue());

    /// <inheritdoc />
    public override Node VisitRun_at_stage(sassy_parser.Run_at_stageContext context) =>
        new RunAtStageAttribute(context.GetCoordinate(), context.stage.GetStringValue());

    public override Node VisitNew_asset(sassy_parser.New_assetContext context)
    {
        var expressions = context.constructor_arguments().expression().Select(Visit).Cast<Expression>().ToList();
        return new NewAttribute(context.GetCoordinate(), expressions);
    }

    /// <inheritdoc />
    public override Node VisitSel_element(sassy_parser.Sel_elementContext context)
        => new ElementSelector(context.GetCoordinate(), context.ELEMENT().GetText());

    public override Node VisitSel_element_string(sassy_parser.Sel_element_stringContext context) =>
        new ElementSelector(context.GetCoordinate(), context.STRING().GetText().Unescape());

    /// <inheritdoc />
    public override Node VisitSel_ruleset(sassy_parser.Sel_rulesetContext context)
        => new RulesetSelector(context.GetCoordinate(), context.RULESET().GetText().TrimFirst());


    /// <inheritdoc />
    public override Node VisitSel_class_capture(sassy_parser.Sel_class_captureContext context) =>
        new ClassCaptureSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst(),
            context.function_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitSel_string_class_capture(sassy_parser.Sel_string_class_captureContext context) =>
        new ClassCaptureSelector(context.GetCoordinate(), context.STRING_CLASS().GetText().TrimFirst().Unescape(),
            context.function_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitSel_child(sassy_parser.Sel_childContext context)
        => new ChildSelector(context.GetCoordinate(), Visit(context.parent) as Selector,
            Visit(context.child) as Selector);

    /// <inheritdoc />
    public override Node VisitSel_add_element(sassy_parser.Sel_add_elementContext context)
        => new ElementAdditionSelector(context.GetCoordinate(), context.ELEMENT().GetText());

    /// <inheritdoc />
    public override Node VisitSel_add_string_element(sassy_parser.Sel_add_string_elementContext context)
        => new ElementAdditionSelector(context.GetCoordinate(), context.STRING().GetText().Unescape());

    /// <inheritdoc />
    public override Node VisitSel_class(sassy_parser.Sel_classContext context)
        => new ClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitSel_name(sassy_parser.Sel_nameContext context)
        => new NameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitSel_string_name(sassy_parser.Sel_string_nameContext context) =>
        new NameSelector(context.GetCoordinate(), context.STRING_NAME().GetText().TrimFirst().Unescape());

    /// <inheritdoc />
    public override Node VisitSel_intersection(sassy_parser.Sel_intersectionContext context)
        => new IntersectionSelector(context.GetCoordinate(), Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    /// <inheritdoc />
    public override Node VisitSel_ensure(sassy_parser.Sel_ensureContext context) =>
        new EnsureSelector(context.GetCoordinate(), context.ENSURE().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitSel_string_ensure(sassy_parser.Sel_string_ensureContext context) =>
        new EnsureSelector(context.GetCoordinate(), context.STRING_ENSURE().GetText().TrimFirst().Unescape());

    /// <inheritdoc />
    public override Node VisitSel_everything(sassy_parser.Sel_everythingContext context)
        => new WildcardSelector(context.GetCoordinate());

    /// <inheritdoc />
    public override Node VisitSel_without_class(sassy_parser.Sel_without_classContext context)
        => new WithoutClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitSel_without_string_class(sassy_parser.Sel_without_string_classContext context) =>
        new WithoutClassSelector(context.GetCoordinate(), context.STRING_CLASS().GetText().TrimFirst().Unescape());

    /// <inheritdoc />
    public override Node VisitSel_without_name(sassy_parser.Sel_without_nameContext context)
        => new WithoutNameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitSel_without_string_name(sassy_parser.Sel_without_string_nameContext context)
        => new WithoutNameSelector(context.GetCoordinate(), context.STRING_NAME().GetText().TrimFirst().Unescape());

    /// <inheritdoc />
    public override Node VisitSel_combination(sassy_parser.Sel_combinationContext context)
        => new CombinationSelector(context.GetCoordinate(), Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    /// <inheritdoc />
    public override Node VisitRuleset_selector(sassy_parser.Ruleset_selectorContext context)
        => new RulesetSelector(context.GetCoordinate(), context.RULESET().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitCombination_selector(sassy_parser.Combination_selectorContext context)
        => new CombinationSelector(context.GetCoordinate(), Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    /// <inheritdoc />
    public override Node VisitClass_capture_selector(sassy_parser.Class_capture_selectorContext context) =>
        new ClassCaptureSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst(),
            context.function_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitString_class_capture_selector(
        sassy_parser.String_class_capture_selectorContext context
    ) =>
        new ClassCaptureSelector(context.GetCoordinate(), context.STRING_CLASS().GetText().TrimFirst().Unescape(),
            context.function_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitWithout_name(sassy_parser.Without_nameContext context)
        => new WithoutNameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitWithout_string_name(sassy_parser.Without_string_nameContext context) =>
        new WithoutNameSelector(context.GetCoordinate(), context.STRING_NAME().GetText().TrimFirst().Unescape());

    /// <inheritdoc />
    public override Node VisitClass_selector(sassy_parser.Class_selectorContext context)
        => new ClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitString_class_selector(sassy_parser.String_class_selectorContext context) =>
        new ClassSelector(context.GetCoordinate(), context.STRING_CLASS().GetText().TrimFirst().Unescape());

    /// <inheritdoc />
    public override Node VisitWithout_class(sassy_parser.Without_classContext context)
        => new WithoutClassSelector(context.GetCoordinate(), context.CLASS().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitWithout_string_class(sassy_parser.Without_string_classContext context) =>
        new WithoutClassSelector(context.GetCoordinate(), context.STRING_CLASS().GetText().TrimFirst().Unescape());

    /// <inheritdoc />
    public override Node VisitName(sassy_parser.NameContext context)
        => new NameSelector(context.GetCoordinate(), context.NAME().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitString_name(sassy_parser.String_nameContext context)
        => new NameSelector(context.GetCoordinate(), context.STRING_NAME().GetText().TrimFirst().Unescape());

    /// <inheritdoc />
    public override Node VisitAdd_element(sassy_parser.Add_elementContext context)
        => new ElementAdditionSelector(context.GetCoordinate(), context.ELEMENT().GetText());

    /// <inheritdoc />
    public override Node VisitAdd_string_element(sassy_parser.Add_string_elementContext context)
        => new ElementAdditionSelector(context.GetCoordinate(), context.STRING().GetText().Unescape());

    /// <inheritdoc />
    public override Node VisitEverything(sassy_parser.EverythingContext context)
        => new WildcardSelector(context.GetCoordinate());

    /// <inheritdoc />
    public override Node VisitIntersection_selector(sassy_parser.Intersection_selectorContext context)
        => new IntersectionSelector(context.GetCoordinate(), Visit(context.lhs) as Selector,
            Visit(context.rhs) as Selector);

    /// <inheritdoc />
    public override Node VisitElement(sassy_parser.ElementContext context)
        => new ElementSelector(context.GetCoordinate(), context.ELEMENT().GetText());

    /// <inheritdoc />
    public override Node VisitString_element(sassy_parser.String_elementContext context) =>
        new ElementSelector(context.GetCoordinate(), context.STRING().GetText().Unescape());

    /// <inheritdoc />
    public override Node VisitSel_level_conditional(sassy_parser.Sel_level_conditionalContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }

        return new Conditional(context.GetCoordinate(),
            Visit(context.cond) as Expression,
            context.selector_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    /// <inheritdoc />
    public override Node VisitSel_level_else_else(sassy_parser.Sel_level_else_elseContext context) =>
        new Block(context.GetCoordinate(),
            context.selector_statement()
                .Select(Visit)
                .ToList());

    /// <inheritdoc />
    public override Node VisitSel_level_else_if(sassy_parser.Sel_level_else_ifContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }

        return new Conditional(context.GetCoordinate(),
            Visit(context.cond) as Expression,
            context.selector_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    /// <inheritdoc />
    public override Node VisitSet_value(sassy_parser.Set_valueContext context)
        => new SetValue(context.GetCoordinate(), Visit(context.expr) as Expression);

    /// <inheritdoc />
    public override Node VisitDelete_value(sassy_parser.Delete_valueContext context)
        => new DeleteValue(context.GetCoordinate());

    /// <inheritdoc />
    public override Node VisitMerge_value(sassy_parser.Merge_valueContext context)
        => new MergeValue(context.GetCoordinate(), Visit(context.expr) as Expression);

    /// <inheritdoc />
    public override Node VisitNormal_field_set(sassy_parser.Normal_field_setContext context) =>
        new Field(context.GetCoordinate(),
            context.sassy_string().GetStringValue(),
            context.indexor != null
                ? Visit(context.indexor) as Indexer
                : null,
            Visit(context.expr) as Expression);

    /// <inheritdoc />
    public override Node VisitAdd_field_set(sassy_parser.Add_field_setContext context) =>
        new Field(context.GetCoordinate(),
            context.sassy_string().GetStringValue(),
            context.indexor != null
                ? Visit(context.indexor) as Indexer
                : null,
            new ImplicitAdd(context.GetCoordinate(), Visit(context.expr) as Expression));

    /// <inheritdoc />
    public override Node VisitSubtract_field_set(sassy_parser.Subtract_field_setContext context) =>
        new Field(context.GetCoordinate(),
            context.sassy_string().GetStringValue(),
            context.indexor != null
                ? Visit(context.indexor) as Indexer
                : null,
            new ImplicitSubtract(context.GetCoordinate(), Visit(context.expr) as Expression));

    /// <inheritdoc />
    public override Node VisitMultiply_field_set(sassy_parser.Multiply_field_setContext context) =>
        new Field(context.GetCoordinate(),
            context.sassy_string().GetStringValue(),
            context.indexor != null
                ? Visit(context.indexor) as Indexer
                : null,
            new ImplicitMultiply(context.GetCoordinate(), Visit(context.expr) as Expression));


    /// <inheritdoc />
    public override Node VisitDivide_field_set(sassy_parser.Divide_field_setContext context) =>
        new Field(context.GetCoordinate(),
            context.sassy_string().GetStringValue(),
            context.indexor != null
                ? Visit(context.indexor) as Indexer
                : null,
            new ImplicitDivide(context.GetCoordinate(), Visit(context.expr) as Expression));


    /// <inheritdoc />
    public override Node VisitNumber_indexor(sassy_parser.Number_indexorContext context)
    {
        var location = context.GetCoordinate();
        return ulong.TryParse(context.num.Text, NumberStyles.Number, CultureInfo.InvariantCulture,
            out var index)
            ? new NumberIndexer(location,
                index)
            : Error(location,
                "Index must be a positive integer");
    }

    /// <inheritdoc />
    public override Node VisitElement_indexor(sassy_parser.Element_indexorContext context) =>
        new ElementIndexer(context.GetCoordinate(), context.elem.Text);

    /// <inheritdoc />
    public override Node VisitClass_indexor(sassy_parser.Class_indexorContext context) =>
        new ClassIndexer(context.GetCoordinate(), context.clazz.Text.TrimFirst());

    /// <inheritdoc />
    public override Node VisitString_indexor(sassy_parser.String_indexorContext context) =>
        new StringIndexer(context.GetCoordinate(), context.elem.Text.Unescape());

    /// <inheritdoc />
    public override Node VisitNot_equal_to(sassy_parser.Not_equal_toContext context) =>
        new NotEqualTo(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitVariable_reference(sassy_parser.Variable_referenceContext context) =>
        new VariableReference(context.GetCoordinate(), context.VARIABLE().GetText().TrimFirst());

    /// <inheritdoc />
    public override Node VisitEqual_to(sassy_parser.Equal_toContext context) =>
        new EqualTo(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitLocal_variable_reference(sassy_parser.Local_variable_referenceContext context) =>
        new LocalVariableReference(context.GetCoordinate(), context.LOCALVARIABLE().GetText().TrimFirst().TrimFirst());


    /// <inheritdoc />
    public override Node VisitIndexor(sassy_parser.IndexorContext context) =>
        new Subscript(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitOr(sassy_parser.OrContext context) =>
        new Or(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitLesser_than_equal(sassy_parser.Lesser_than_equalContext context) =>
        new LesserThanEqual(context.GetCoordinate(), Visit(context.lhs) as Expression,
            Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitSubtraction(sassy_parser.SubtractionContext context) =>
        new Subtract(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitPositive(sassy_parser.PositiveContext context) =>
        new Positive(context.GetCoordinate(), Visit(context.child) as Expression);

    /// <inheritdoc />
    public override Node VisitSimple_call(sassy_parser.Simple_callContext context) =>
        new SimpleCall(context.GetCoordinate(), context.lhs.Text,
            context.args.argument().Select(Visit).Cast<CallArgument>().ToList());

    /// <inheritdoc />
    public override Node VisitDivision(sassy_parser.DivisionContext context) =>
        new Divide(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitNegative(sassy_parser.NegativeContext context) =>
        new Negate(context.GetCoordinate(), Visit(context.child) as Expression);

    /// <inheritdoc />
    public override Node VisitNot(sassy_parser.NotContext context) =>
        new Not(context.GetCoordinate(), Visit(context.child) as Expression);

    /// <inheritdoc />
    public override Node VisitLesser_than(sassy_parser.Lesser_thanContext context) =>
        new LesserThan(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitMember_call(sassy_parser.Member_callContext context) =>
        new MemberCall(context.GetCoordinate(), Visit(context.lhs) as Expression, context.name.Text,
            context.args.argument().Select(Visit).Cast<CallArgument>().ToList());

    /// <inheritdoc />
    public override Node VisitMember_call_ruleset(sassy_parser.Member_call_rulesetContext context) =>
        new MemberCall(context.GetCoordinate(), Visit(context.lhs) as Expression,
            context.RULESET().GetText().TrimFirst(),
            context.args.argument().Select(Visit).Cast<CallArgument>().ToList());

    /// <inheritdoc />
    public override Node VisitGreater_than(sassy_parser.Greater_thanContext context) =>
        new GreaterThan(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitAnd(sassy_parser.AndContext context) =>
        new And(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitMultiplication(sassy_parser.MultiplicationContext context) =>
        new Multiply(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitRemainder(sassy_parser.RemainderContext context) =>
        new Remainder(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitGreater_than_equal(sassy_parser.Greater_than_equalContext context) =>
        new GreaterThanEqual(context.GetCoordinate(), Visit(context.lhs) as Expression,
            Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitTernary(sassy_parser.TernaryContext context) =>
        new Ternary(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.cond) as Expression,
            Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitAddition(sassy_parser.AdditionContext context) =>
        new Add(context.GetCoordinate(), Visit(context.lhs) as Expression, Visit(context.rhs) as Expression);

    /// <inheritdoc />
    public override Node VisitValue_deletion(sassy_parser.Value_deletionContext context)
        => new ValueNode(context.GetCoordinate(), new DataValue(DataValue.DataType.Deletion));

    /// <inheritdoc />
    public override Node VisitBoolean_true(sassy_parser.Boolean_trueContext context)
        => new ValueNode(context.GetCoordinate(), true);

    /// <inheritdoc />
    public override Node VisitBoolean_false(sassy_parser.Boolean_falseContext context)
        => new ValueNode(context.GetCoordinate(), false);

    /// <inheritdoc />
    public override Node VisitNumber_value(sassy_parser.Number_valueContext context)
    {
        var location = context.GetCoordinate();
        if (long.TryParse(context.NUMBER().GetText(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var lng))
        {
            return new ValueNode(location, lng);
        }

        if (double.TryParse(context.NUMBER().GetText(), NumberStyles.Number, CultureInfo.InvariantCulture, out var dbl))
        {
            return new ValueNode(location, dbl);
        }

        Debug.Log($"Number: {context.NUMBER().GetText()}");
        return Error(location, "Numbers must be parsable as a double precision floating point number");
    }

    /// <inheritdoc />
    public override Node VisitString_value(sassy_parser.String_valueContext context)
        => new ValueNode(context.GetCoordinate(), context.STRING().GetText().Unescape());

    /// <inheritdoc />
    public override Node VisitNone(sassy_parser.NoneContext context)
        => new ValueNode(context.GetCoordinate(), new DataValue(DataValue.DataType.None));

    /// <inheritdoc />
    public override Node VisitList_value(sassy_parser.List_valueContext context)
        => new ListNode(context.GetCoordinate(),
            context.list().list_values().expression().Select(Visit).Cast<Expression>().ToList());

    /// <inheritdoc />
    public override Node VisitObject_value(sassy_parser.Object_valueContext context)
        => new ObjectNode(context.GetCoordinate(),
            context.obj().obj_values().key_value().Select(Visit).Cast<KeyValueNode>().ToList());

    /// <inheritdoc />
    public override Node VisitLiteral_key(sassy_parser.Literal_keyContext context)
        => new KeyValueNode(context.GetCoordinate(), context.key.Text, Visit(context.val) as Expression);

    /// <inheritdoc />
    public override Node VisitString_key(sassy_parser.String_keyContext context)
        => new KeyValueNode(context.GetCoordinate(), context.key.Text.Unescape(), Visit(context.val) as Expression);

    /// <inheritdoc />
    public override Node VisitNamed_argument(sassy_parser.Named_argumentContext context)
        => new CallArgument(context.GetCoordinate(), context.key.Text.TrimFirst(), Visit(context.val) as Expression);

    /// <inheritdoc />
    public override Node VisitUnnamed_argument(sassy_parser.Unnamed_argumentContext context)
        => new CallArgument(context.GetCoordinate(), Visit(context.val) as Expression);

    /// <inheritdoc />
    public override Node VisitFn_level_conditional(sassy_parser.Fn_level_conditionalContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }

        return new Conditional(context.GetCoordinate(),
            Visit(context.cond) as Expression,
            context.function_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    /// <inheritdoc />
    public override Node VisitFn_level_else_else(sassy_parser.Fn_level_else_elseContext context) =>
        new Block(context.GetCoordinate(),
            context.function_statement()
                .Select(Visit)
                .ToList());

    /// <inheritdoc />
    public override Node VisitFn_level_else_if(sassy_parser.Fn_level_else_ifContext context)
    {
        Node @else = null;
        if (context.els != null)
        {
            @else = Visit(context.els);
        }

        return new Conditional(context.GetCoordinate(),
            Visit(context.cond) as Expression,
            context.function_statement()
                .Select(Visit)
                .ToList(),
            @else);
    }

    /// <inheritdoc />
    public override Node VisitFn_return(sassy_parser.Fn_returnContext context)
    {
        return new Return(context.GetCoordinate(), Visit(context.expression()) as Expression);
    }

    /// <inheritdoc />
    public override Node VisitMixin_include(sassy_parser.Mixin_includeContext context) =>
        new MixinInclude(context.GetCoordinate(), context.mixin.Text,
            context.args.argument().Select(Visit).Cast<CallArgument>().ToList());

    /// <inheritdoc />
    public override Node VisitArgument_without_default(sassy_parser.Argument_without_defaultContext context)
        => new Argument(context.GetCoordinate(), context.name.Text.TrimFirst());

    /// <inheritdoc />
    public override Node VisitArgument_with_default(sassy_parser.Argument_with_defaultContext context)
        => new Argument(context.GetCoordinate(), context.name.Text.TrimFirst(), Visit(context.val) as Expression);

    /// <inheritdoc />
    public override Node VisitSel_sub(sassy_parser.Sel_subContext context) =>
        Visit(context.internal_selector);

    /// <inheritdoc />
    public override Node VisitSub_selector(sassy_parser.Sub_selectorContext context) =>
        Visit(context.internal_selector);

    /// <inheritdoc />
    public override Node VisitSub_sub_expression(sassy_parser.Sub_sub_expressionContext context) =>
        Visit(context.internal_expr);

    /// <inheritdoc />
    public override Node VisitFor_to_loop(sassy_parser.For_to_loopContext context)
        => new For(context.GetCoordinate(), context.idx.Text.TrimFirst(), Visit(context.for_start) as Expression, false,
            Visit(context.end) as Expression, context.function_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitTop_level_for_to_loop(sassy_parser.Top_level_for_to_loopContext context) => new For(
        context.GetCoordinate(), context.idx.Text.TrimFirst(), Visit(context.for_start) as Expression, false,
        Visit(context.end) as Expression, context.top_level_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitSel_level_for_to_loop(sassy_parser.Sel_level_for_to_loopContext context) => new For(
        context.GetCoordinate(), context.idx.Text.TrimFirst(), Visit(context.for_start) as Expression, false,
        Visit(context.end) as Expression, context.selector_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitFor_through_loop(sassy_parser.For_through_loopContext context)
        => new For(context.GetCoordinate(), context.idx.Text.TrimFirst(), Visit(context.for_start) as Expression, true,
            Visit(context.end) as Expression, context.function_statement().Select(Visit).ToList());


    /// <inheritdoc />
    public override Node VisitTop_level_for_through_loop(sassy_parser.Top_level_for_through_loopContext context)
        => new For(context.GetCoordinate(), context.idx.Text.TrimFirst(), Visit(context.for_start) as Expression, true,
            Visit(context.end) as Expression, context.top_level_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitSel_level_for_through_loop(sassy_parser.Sel_level_for_through_loopContext context)
        => new For(context.GetCoordinate(), context.idx.Text.TrimFirst(), Visit(context.for_start) as Expression, true,
            Visit(context.end) as Expression, context.selector_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitEach_loop(sassy_parser.Each_loopContext context)
        => new Each(context.GetCoordinate(), context.key?.Text.TrimFirst(), context.val.Text.TrimFirst(),
            Visit(context.iter) as Expression, context.function_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitTop_level_each_loop(sassy_parser.Top_level_each_loopContext context)
        => new Each(context.GetCoordinate(), context.key?.Text.TrimFirst(), context.val.Text.TrimFirst(),
            Visit(context.iter) as Expression, context.top_level_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitSel_level_each_loop(sassy_parser.Sel_level_each_loopContext context)
        => new Each(context.GetCoordinate(), context.key?.Text.TrimFirst(), context.val.Text.TrimFirst(),
            Visit(context.iter) as Expression, context.selector_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitWhile_loop(sassy_parser.While_loopContext context)
        => new While(context.GetCoordinate(), Visit(context.cond) as Expression,
            context.function_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitTop_level_while_loop(sassy_parser.Top_level_while_loopContext context)
        => new While(context.GetCoordinate(), Visit(context.cond) as Expression,
            context.top_level_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitSel_level_while_loop(sassy_parser.Sel_level_while_loopContext context)
        => new While(context.GetCoordinate(), Visit(context.cond) as Expression,
            context.selector_statement().Select(Visit).ToList());

    /// <inheritdoc />
    public override Node VisitGlobal_stage_def(sassy_parser.Global_stage_defContext context) =>
        new StageDefinition(context.GetCoordinate(), context.stage.GetStringValue(), true);

    /// <inheritdoc />
    public override Node VisitImplicit_stage_def(sassy_parser.Implicit_stage_defContext context) =>
        new StageDefinition(context.GetCoordinate(), context.stage.GetStringValue());

    /// <inheritdoc />
    public override Node VisitRelative_stage_def(sassy_parser.Relative_stage_defContext context) => new StageDefinition(
        context.GetCoordinate(), context.stage.GetStringValue(),
        context.stage_attribute().Select(Visit).Cast<StageDefinitionAttribute>().ToList());

    /// <inheritdoc />
    public override Node VisitConfig_creation(sassy_parser.Config_creationContext context) => new ConfigCreation(
        context.GetCoordinate(), context.label.GetStringValue(), context.config_name.GetStringValue(),
        Visit(context.config_value) as Expression);

    /// <inheritdoc />
    public override Node VisitElement_string(sassy_parser.Element_stringContext context) =>
        new ValueNode(context.GetCoordinate(), context.ELEMENT().GetText());

    /// <inheritdoc />
    public override Node VisitUpdate_config_full(sassy_parser.Update_config_fullContext context) => new ConfigUpdate(
        context.GetCoordinate(), Visit(context.priority) as Expression, context.label.GetStringValue(),
        context.config_name.GetStringValue(), Visit(context.config_update) as Expression);

    /// <inheritdoc />
    public override Node VisitUpdate_config_label(sassy_parser.Update_config_labelContext context) => new ConfigUpdate(
        context.GetCoordinate(), Visit(context.priority) as Expression, context.label.GetStringValue(), null,
        Visit(context.config_update) as Expression);
}