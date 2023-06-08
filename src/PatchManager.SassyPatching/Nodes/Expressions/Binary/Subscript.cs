using PatchManager.SassyPatching.Execptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class Subscript : Binary
{
    public Subscript(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    public override Value GetResult(Value leftHandSide, Value rightHandSide)
    {
        if (leftHandSide.IsList && rightHandSide.IsNumber)
        {
            return leftHandSide.List[(int)rightHandSide.Number];
        }

        if (leftHandSide.IsString && rightHandSide.IsNumber)
        {
            return (double)leftHandSide.String[(int)rightHandSide.Number];
        }

        if (leftHandSide.IsDictionary && rightHandSide.IsString)
        {
            return leftHandSide.Dictionary[rightHandSide.String];
        }

        throw new BinaryExpressionTypeException("subscript", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }

    public override bool ShortCircuitOn(Value value) => false;

    public override Value ShortCircuitValue => null;
}