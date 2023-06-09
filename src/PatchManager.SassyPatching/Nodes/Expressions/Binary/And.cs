namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

/// <summary>
/// Represents a binary expression that returns true if both children are truthy (short circuits)
/// </summary>
/// <seealso cref="Value.Truthy"/>
public class And : Binary
{
    public And(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    internal override Value GetResult(Value leftHandSide, Value rightHandSide) => rightHandSide.Truthy;

    internal override bool ShortCircuitOn(Value value) => !value.Truthy;

    internal override Value ShortCircuitValue => false;
}