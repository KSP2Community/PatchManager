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
        try
        {
            return leftHandSide.Real - rightHandSide.Integer;
        }
        catch (DataValueOperationException)
        {
            throw new BinaryExpressionTypeException(Coordinate, "subtract", leftHandSide.Type.ToString(),
                rightHandSide.Type.ToString());
        }
    }
}