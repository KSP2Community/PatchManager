using System.Text;
using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

/// <summary>
/// Represents an implicit multiplication which multiplies $value by its child
/// </summary>
public class ImplicitMultiply : Implicit
{
    internal ImplicitMultiply(Coordinate c, Expression child) : base(c, child)
    {
    }

    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {        
        try
        {
            return leftHandSide * rightHandSide;
        }
        catch (DataValueOperationException)
        {
            throw new BinaryExpressionTypeException(Coordinate, "multiply", leftHandSide.Type.ToString(),
                rightHandSide.Type.ToString());
        }
    }
}