using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

/// <summary>
/// Represents a binary expression which adds its 2 children together and returns the result
/// </summary>
public class Add : Binary
{
    internal Add(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

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

    internal override bool ShortCircuitOn(DataValue dataValue) => false;
    internal override DataValue ShortCircuitDataValue => null;
}