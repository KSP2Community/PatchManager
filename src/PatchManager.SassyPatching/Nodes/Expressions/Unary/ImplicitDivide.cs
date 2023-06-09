using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

/// <summary>
/// Represent an implicit division which divides $value by its child
/// </summary>
public class ImplicitDivide : Implicit
{
    internal ImplicitDivide(Coordinate c, Expression child) : base(c, child)
    {
    }

    internal override Value GetResult(Value leftHandSide, Value rightHandSide)
    {
        if (leftHandSide.IsNumber && rightHandSide.IsNumber)
        {
            return leftHandSide.Number / rightHandSide.Number;
        }

        throw new BinaryExpressionTypeException(Coordinate,"divide", leftHandSide.Type.ToString(), rightHandSide.Type.ToString());
    }
}