namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class And : Binary
{
    public And(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    public override Value GetResult(Value leftHandSide, Value rightHandSide) => rightHandSide.Truthy;

    public override bool ShortCircuitOn(Value value) => !value.Truthy;

    public override Value ShortCircuitValue => false;
}