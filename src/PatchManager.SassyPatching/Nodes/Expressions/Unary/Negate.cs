using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

public class Negate : Unary
{
    public Negate(Coordinate c, Expression child) : base(c, child)
    {
    }

    public override Value GetResult(Value child)
    {
        if (child.IsNumber)
        {
            return -child.Number;
        }

        throw new UnaryTypeException(Coordinate, "negate", child.Type.ToString());
    }
}