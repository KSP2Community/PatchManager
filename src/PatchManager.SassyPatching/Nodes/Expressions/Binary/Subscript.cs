using System.Globalization;
using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class Subscript : Binary
{
    public Subscript(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    public override Value GetResult(Value leftHandSide, Value rightHandSide)
    {
        if (leftHandSide.IsList && rightHandSide.IsNumber)
        {
            try
            {
                return leftHandSide.List[(int)rightHandSide.Number];
            }
            catch
            {
                throw new ListIndexOutOfRangeException(Coordinate,
                    ((int)rightHandSide.Number) + " is out of range of the list being indexed");
            }
        }

        if (leftHandSide.IsString && rightHandSide.IsNumber)
        {
            try
            {
                return (double)leftHandSide.String[(int)rightHandSide.Number];
            }
            catch
            {
                throw new ListIndexOutOfRangeException(Coordinate,
                    ((int)rightHandSide.Number) + " is out of range of the string being indexed");
            }
        }

        if (leftHandSide.IsDictionary && rightHandSide.IsString)
        {
            try
            {
                return leftHandSide.Dictionary[rightHandSide.String];
            }
            catch
            {
                throw new DictionaryKeyNotFoundException(Coordinate,
                    rightHandSide.String + " is not a key found in the dictionary being indexed");
            }
        }

        throw new BinaryExpressionTypeException(Coordinate, "subscript", leftHandSide.Type.ToString(),
            rightHandSide.Type.ToString());
    }

    public override bool ShortCircuitOn(Value value) => false;

    public override Value ShortCircuitValue => null;
}