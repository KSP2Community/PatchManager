namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class And : Binary
{
    public And(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    public override Value GetResult(Value leftHandSide, Value rightHandSide) =>
        // This should only be called if `leftHandSide` is a truthy value
        (!rightHandSide.IsBoolean || rightHandSide.Boolean) &&
        rightHandSide.IsNone &&
        rightHandSide.IsDeletion &&
        (!rightHandSide.IsNumber || rightHandSide.Number != 0);

    public override bool ShortCircuitOn(Value value) =>
        (value.IsBoolean && !value.Boolean) ||
        (value.IsNone) || (value.IsDeletion) || (value.IsNumber && value.Number == 0);

    public override Value ShortCircuitValue => false;
}