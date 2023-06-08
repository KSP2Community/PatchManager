using PatchManager.SassyPatching.Execptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class GreaterThanEqual : Binary
{
    public GreaterThanEqual(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    public override Value GetResult(Value leftHandSide, Value rightHandSide)
    { 
        if (leftHandSide.IsNumber && rightHandSide.IsNumber)
        {
            return leftHandSide.Number >= rightHandSide.Number;
        }

        if (leftHandSide.IsString && rightHandSide.IsString)
        {
            return string.Compare(leftHandSide.String, rightHandSide.String, StringComparison.Ordinal) >= 0;
        }

        throw new BinaryExpressionTypeException("perform a relational comparison (>=)", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }

    public override bool ShortCircuitOn(Value value) => false;

    public override Value ShortCircuitValue => null;
}