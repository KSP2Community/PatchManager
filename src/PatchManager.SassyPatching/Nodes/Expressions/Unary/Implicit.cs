using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

/// <summary>
/// <para>
/// Represents an implicit operation node, which is a node that grabs the variable $value from the environment and applies a computation to it
/// </para>
/// <para>
/// $value is always the value of the variable/field being modified
/// </para>
/// </summary>
public abstract class Implicit : Unary
{
    internal Implicit(Coordinate c, Expression child) : base(c, child)
    {
    }
    internal abstract Value GetResult(Value leftHandSide, Value rightHandSide);

    internal override Value GetResult(Value child)
    {
        throw new Exception("Unreachable");
    }

    /// <inheritdoc />
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