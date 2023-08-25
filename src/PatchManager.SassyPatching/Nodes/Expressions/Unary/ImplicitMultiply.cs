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
        if (leftHandSide.IsReal && rightHandSide.IsReal)
        {
            return leftHandSide.Real * rightHandSide.Real;
        }

        if (leftHandSide.IsReal && rightHandSide.IsInteger)
        {
            return leftHandSide.Real * rightHandSide.Integer;
        }

        if (leftHandSide.IsInteger && rightHandSide.IsInteger)
        {
            return leftHandSide.Integer * rightHandSide.Integer;
        }

        if (leftHandSide.IsInteger && rightHandSide.IsReal)
        {
            return leftHandSide.Integer * rightHandSide.Real;
        }
        if (leftHandSide.IsString && rightHandSide.IsInteger)
        {
            return StringRepeat(leftHandSide, rightHandSide);
        }

        if (leftHandSide.IsInteger && rightHandSide.IsString)
        {
            return StringRepeat(rightHandSide, leftHandSide);
        }
        
        if (leftHandSide.IsList && rightHandSide.IsInteger)
        {
            return ListRepeat(leftHandSide, rightHandSide);
        }

        if (leftHandSide.IsInteger && rightHandSide.IsList)
        {
            return ListRepeat(rightHandSide, leftHandSide);
        }

        throw new BinaryExpressionTypeException(Coordinate,"multiply", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }
    
    private static DataValue StringRepeat(DataValue str, DataValue amount)
    {
        StringBuilder sb = new StringBuilder();
        for (var i = 0; i < amount.Integer; i++)
        {
            sb.Append(str.String);
        }

        return sb.ToString();
    }

    private static DataValue ListRepeat(DataValue list, DataValue amount)
    {
        List<DataValue> newList = new();
        for (var i = 0; i < amount.Integer; i++)
        {
            newList.AddRange(list.List);
        }
        return newList;
    }
}