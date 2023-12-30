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
        if (leftHandSide.IsReal && rightHandSide.IsReal)
        {
            return leftHandSide.Real - rightHandSide.Real;
        }

        if (leftHandSide.IsReal && rightHandSide.IsInteger)
        {
            return leftHandSide.Real - rightHandSide.Integer;
        }

        if (leftHandSide.IsInteger && rightHandSide.IsInteger)
        {
            return leftHandSide.Integer - rightHandSide.Integer;
        }

        if (leftHandSide.IsInteger && rightHandSide.IsReal)
        {
            return leftHandSide.Integer - rightHandSide.Real;
        }

        
        if (leftHandSide.IsList && rightHandSide.IsList)
        {
            return leftHandSide.List.Where(x => rightHandSide.List.All(y => x != y)).ToList();
        }

        
        throw new BinaryExpressionTypeException(Coordinate,"subtract", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }
}