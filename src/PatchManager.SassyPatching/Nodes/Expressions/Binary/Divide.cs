using PatchManager.SassyPatching.Execptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class Divide : Binary
{
    public Divide(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    public override Value GetResult(Value leftHandSide, Value rightHandSide)
    {
        if (leftHandSide.IsNumber && rightHandSide.IsNumber)
        {
            return leftHandSide.Number / rightHandSide.Number;
        }

        throw new BinaryExpressionTypeException("divide", leftHandSide.Type.ToString(), rightHandSide.Type.ToString());
    }

    public override bool ShortCircuitOn(Value value) => false;
    public override Value ShortCircuitValue => null;
}