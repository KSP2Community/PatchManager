namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents a ternary expression, which computes the left hand side if the condition is true otherwise the righthand side
/// </summary>
public class Ternary : Expression
{
    /// <summary>
    /// The expression to be computed if the condition is true
    /// </summary>
    public readonly Expression LeftHandSide;
    /// <summary>
    /// The condition to be tested
    /// </summary>
    public readonly Expression Condition;
    /// <summary>
    /// The expression to be computed if the condition is false
    /// </summary>
    public readonly Expression RightHandSide;

    internal Ternary(Coordinate c, Expression leftHandSide, Expression condition, Expression rightHandSide) : base(c)
    {
        LeftHandSide = leftHandSide;
        Condition = condition;
        RightHandSide = rightHandSide;
    }

    /// <inheritdoc />
    public override Value Compute(Environment environment) => Condition.Compute(environment).Truthy
        ? LeftHandSide.Compute(environment)
        : RightHandSide.Compute(environment);
}