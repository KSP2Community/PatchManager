using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

/// <summary>
/// Represents an implicit subtraction which subtracts its child from $value
/// </summary>
public class ImplicitSubtract : Implicit
{
    internal ImplicitSubtract(Coordinate c, Expression child) : base(c, child)
    {
    }

    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {
        if (leftHandSide.IsNumber && rightHandSide.IsNumber)
        {
            return leftHandSide.Number - rightHandSide.Number;
        }

        throw new BinaryExpressionTypeException(Coordinate,"subtract", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }
}