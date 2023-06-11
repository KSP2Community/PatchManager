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
        switch (leftHandSide.Type)
        {
            case DataValue.DataType.Real when rightHandSide.IsReal:
                return leftHandSide.Real + rightHandSide.Real;
            case DataValue.DataType.Real when rightHandSide.IsInteger:
                return leftHandSide.Real + rightHandSide.Integer;
            case DataValue.DataType.Integer when rightHandSide.IsInteger:
                return leftHandSide.Integer + rightHandSide.Integer;
            case DataValue.DataType.Integer when rightHandSide.IsReal:
                return leftHandSide.Integer + rightHandSide.Real;
            case DataValue.DataType.String when rightHandSide.IsString:
                return leftHandSide.String + rightHandSide.String;
            case DataValue.DataType.List when rightHandSide.IsList:
            {
                // If every value is immutable a shallow copy should be fine
                var newList = new List<DataValue>(leftHandSide.List);
                newList.AddRange(rightHandSide.List);
                return newList;
            }
            case DataValue.DataType.None:
            case DataValue.DataType.Boolean:
            case DataValue.DataType.Dictionary:
            case DataValue.DataType.Deletion:
            default:
                throw new BinaryExpressionTypeException(Coordinate,"add", leftHandSide.Type.ToString(), rightHandSide.Type.ToString());
        }
    }

    internal override bool ShortCircuitOn(DataValue dataValue) => false;
    internal override DataValue ShortCircuitDataValue => null;
}