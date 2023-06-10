using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

/// <summary>
/// Represents a binary expression that returns true if its left hand side is greater than its right hand side
/// </summary>
public class GreaterThan : Binary
{
    internal GreaterThan(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {
        if (leftHandSide.IsNumber && rightHandSide.IsNumber)
        {
            return leftHandSide.Number > rightHandSide.Number;
        }

        if (leftHandSide.IsString && rightHandSide.IsString)
        {
            return string.Compare(leftHandSide.String, rightHandSide.String, StringComparison.Ordinal) > 0;
        }

        throw new BinaryExpressionTypeException(Coordinate,"perform a relational comparison (>)", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }

    internal override bool ShortCircuitOn(DataValue dataValue) => false;

    internal override DataValue ShortCircuitDataValue => null;
}