using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

public abstract class Implicit : Unary
{
    public Implicit(Coordinate c, Expression child) : base(c, child)
    {
    }

    public abstract Value GetResult(Value leftHandSide, Value rightHandSide);
    public override Value GetResult(Value child)
    {
        throw new Exception("Unreachable");
    }

    public override Value Compute(Environment environment)
    {
        try
        {
            return GetResult(environment["value"], Child.Compute(environment));
        }
        catch
        {
            throw new InvalidVariableReferenceException(Coordinate, "$value does not exist in current scope");
        }
    }
}