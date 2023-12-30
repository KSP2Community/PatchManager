using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

/// <summary>
/// Represents an implicit addition, which adds the child to $value
/// </summary>
public class ImplicitAdd : Implicit
{
    internal ImplicitAdd(Coordinate c, Expression child) : base(c, child)
    {
    }

    // There should be a value called "$value" in the current environment, a literal new environment gets made here w/ that value set
    internal override DataValue GetResult(DataValue child) => throw
        // Should be unreachable
        new Exception("Unreachable");

    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {
        try
        {
            return leftHandSide + rightHandSide;
        }
        catch (DataValueOperationException)
        {
            throw new BinaryExpressionTypeException(Coordinate, "add", leftHandSide.Type.ToString(),
                rightHandSide.Type.ToString());
        }
    }
    
}