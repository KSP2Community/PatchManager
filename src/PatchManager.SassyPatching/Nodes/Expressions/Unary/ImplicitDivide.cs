using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

public class ImplicitDivide : Implicit
{
    public ImplicitDivide(Coordinate c, Expression child) : base(c, child)
    {
    }

    public override Value GetResult(Value leftHandSide, Value rightHandSide)
    {
        if (leftHandSide.IsNumber && rightHandSide.IsNumber)
        {
            return leftHandSide.Number / rightHandSide.Number;
        }

        throw new BinaryExpressionTypeException(Coordinate,"divide", leftHandSide.Type.ToString(), rightHandSide.Type.ToString());
    }
}