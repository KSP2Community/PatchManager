namespace PatchManager.SassyPatching.Nodes.Expressions;

public class Ternary : Expression
{
    public Expression LeftHandSide;
    public Expression Condition;
    public Expression RightHandSide;

    public Ternary(Coordinate c, Expression leftHandSide, Expression condition, Expression rightHandSide) : base(c)
    {
        LeftHandSide = leftHandSide;
        Condition = condition;
        RightHandSide = rightHandSide;
    }

    public override Value Compute(Environment environment) => Condition.Compute(environment).Truthy
        ? LeftHandSide.Compute(environment)
        : RightHandSide.Compute(environment);
}