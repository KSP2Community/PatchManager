using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;
/// <summary>
/// Represents a binary expression which subtracts the right hand side from the left hand side
/// </summary>
public class Subtract : Binary
{
    internal Subtract(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {
        try
        {
            return leftHandSide - rightHandSide;
        }
        catch (DataValueOperationException)
        {
            throw new BinaryExpressionTypeException(Coordinate, "subtract", leftHandSide.Type.ToString(),
                rightHandSide.Type.ToString());
        }
    }

    internal override bool ShortCircuitOn(DataValue dataValue) => false;

    internal override DataValue ShortCircuitDataValue => null;
}