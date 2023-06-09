using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;
/// <summary>
/// Represents a binary expression that returns true if its left hand side is less than its right hand side
/// </summary>
public class LesserThan : Binary
{
    internal LesserThan(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    internal override Value GetResult(Value leftHandSide, Value rightHandSide)
    {
        if (leftHandSide.IsNumber && rightHandSide.IsNumber)
        {
            return leftHandSide.Number < rightHandSide.Number;
        }

        if (leftHandSide.IsString && rightHandSide.IsString)
        {
            return string.Compare(leftHandSide.String, rightHandSide.String, StringComparison.Ordinal) < 0;
        }

        throw new BinaryExpressionTypeException(Coordinate,"perform a relational comparison (<)", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }

    internal override bool ShortCircuitOn(Value value) => false;

    internal override Value ShortCircuitValue => null;
}