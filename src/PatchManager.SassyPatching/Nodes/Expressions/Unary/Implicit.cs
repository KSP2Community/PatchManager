using Environment = PatchManager.SassyPatching.Execution.Environment;

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
    internal abstract DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide);

    internal override DataValue GetResult(DataValue child)
    {
        throw new Exception("Unreachable");
    }

    /// <inheritdoc />
    public override DataValue Compute(Environment environment) => GetResult(environment["value"], Child.Compute(environment));
}