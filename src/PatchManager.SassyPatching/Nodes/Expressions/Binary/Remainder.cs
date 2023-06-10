using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;
/// <summary>
/// Represents a binary expression which divides the left hand side by the right hand side and returns the remainder
/// </summary>
public class Remainder : Binary
{
    internal Remainder(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {
        if (leftHandSide.IsNumber && rightHandSide.IsNumber)
        {
            return leftHandSide.Number % rightHandSide.Number;
        }

        throw new BinaryExpressionTypeException(Coordinate,"modulate", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }

    internal override bool ShortCircuitOn(DataValue dataValue) => false;

    internal override DataValue ShortCircuitDataValue => null;
}