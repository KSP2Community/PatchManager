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

    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {
        if (leftHandSide.IsReal && rightHandSide.IsReal)
        {
            return leftHandSide.Real / rightHandSide.Real;
        }

        if (leftHandSide.IsInteger && rightHandSide.IsInteger)
        {
            return leftHandSide.Integer / rightHandSide.Integer;
        }

        if (leftHandSide.IsInteger && rightHandSide.IsReal)
        {
            return leftHandSide.Integer / rightHandSide.Real;
        }

        if (leftHandSide.IsReal && rightHandSide.IsInteger)
        {
            return leftHandSide.Real / rightHandSide.Integer;
        }

        throw new BinaryExpressionTypeException(Coordinate,"divide", leftHandSide.Type.ToString(), rightHandSide.Type.ToString());
    }
}