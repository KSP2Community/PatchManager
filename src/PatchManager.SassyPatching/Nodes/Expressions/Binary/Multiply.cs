using System.Text;
using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;
/// <summary>
/// Represents a binary expression which multiplies its 2 children together and returns the result
/// </summary>
public class Multiply : Binary
{
    internal Multiply(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {
        if (leftHandSide.IsNumber && rightHandSide.IsNumber)
        {
            return leftHandSide.Number * rightHandSide.Number;
        }

        if (leftHandSide.IsString && rightHandSide.IsNumber)
        {
            return StringRepeat(leftHandSide, rightHandSide);
        }

        if (leftHandSide.IsNumber && rightHandSide.IsString)
        {
            return StringRepeat(rightHandSide, leftHandSide);
        }
        
        if (leftHandSide.IsList && rightHandSide.IsNumber)
        {
            return ListRepeat(leftHandSide, rightHandSide);
        }

        if (leftHandSide.IsNumber && rightHandSide.IsList)
        {
            return ListRepeat(rightHandSide, leftHandSide);
        }

        throw new BinaryExpressionTypeException(Coordinate,"multiply", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }

    private static DataValue StringRepeat(DataValue str, DataValue amount)
    {
        StringBuilder sb = new StringBuilder();
        for (var i = 0; i < amount.Number; i++)
        {
            sb.Append(str.String);
        }

        return sb.ToString();
    }

    private static DataValue ListRepeat(DataValue list, DataValue amount)
    {
        List<DataValue> newList = new();
        for (var i = 0; i < amount.Number; i++)
        {
            newList.AddRange(list.List);
        }
        return newList;
    }

    internal override bool ShortCircuitOn(DataValue dataValue) => false;

    internal override DataValue ShortCircuitDataValue => null;
}