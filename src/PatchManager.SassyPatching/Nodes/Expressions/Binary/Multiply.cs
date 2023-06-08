using System.Text;
using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class Multiply : Binary
{
    public Multiply(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    public override Value GetResult(Value leftHandSide, Value rightHandSide)
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

    private static Value StringRepeat(Value str, Value amount)
    {
        StringBuilder sb = new StringBuilder();
        for (var i = 0; i < amount.Number; i++)
        {
            sb.Append(str.String);
        }

        return sb.ToString();
    }

    private static Value ListRepeat(Value list, Value amount)
    {
        List<Value> newList = new();
        for (var i = 0; i < amount.Number; i++)
        {
            newList.AddRange(list.List);
        }
        return newList;
    }

    public override bool ShortCircuitOn(Value value) => false;

    public override Value ShortCircuitValue => null;
}