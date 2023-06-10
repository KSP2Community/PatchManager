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
}