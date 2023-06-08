namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class Or : Binary
{
    public Or(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    // This should only be called if the left hand side is falsy
    public override Value GetResult(Value leftHandSide, Value rightHandSide) => rightHandSide.Truthy;

    public override bool ShortCircuitOn(Value value) => value.Truthy;

    public override Value ShortCircuitValue => true;
}