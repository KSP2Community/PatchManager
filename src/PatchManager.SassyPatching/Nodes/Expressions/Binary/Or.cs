namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class Or : Binary
{
    public Or(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    // This should only be called if the left hand side is falsy
    public override Value GetResult(Value leftHandSide, Value rightHandSide) => 
        (rightHandSide.IsBoolean && rightHandSide.Boolean) ||
        (rightHandSide.IsNumber && rightHandSide.Number > 0) || (rightHandSide.IsString) ||
        (rightHandSide.IsList) || (rightHandSide.IsDictionary);

    public override bool ShortCircuitOn(Value value) => (value.IsBoolean && value.Boolean) ||
                                                        (value.IsNumber && value.Number > 0) || (value.IsString) ||
                                                        (value.IsList) || (value.IsDictionary);

    public override Value ShortCircuitValue => true;
}