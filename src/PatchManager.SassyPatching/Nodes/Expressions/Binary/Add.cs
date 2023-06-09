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

    internal override Value GetResult(Value leftHandSide, Value rightHandSide)
    {
        switch (leftHandSide.Type)
        {
            case Value.ValueType.Number when rightHandSide.IsNumber:
                return leftHandSide.Number + rightHandSide.Number;
            case Value.ValueType.String when rightHandSide.IsString:
                return leftHandSide.String + rightHandSide.String;
            case Value.ValueType.List when rightHandSide.IsList:
            {
                // If every value is immutable a shallow copy should be fine
                var newList = new List<Value>(leftHandSide.List);
                newList.AddRange(rightHandSide.List);
                return newList;
            }
            case Value.ValueType.None:
            case Value.ValueType.Boolean:
            case Value.ValueType.Dictionary:
            case Value.ValueType.Deletion:
            default:
                throw new BinaryExpressionTypeException(Coordinate,"add", leftHandSide.Type.ToString(), rightHandSide.Type.ToString());
        }
    }

    internal override bool ShortCircuitOn(Value value) => false;
    internal override Value ShortCircuitValue => null;
}