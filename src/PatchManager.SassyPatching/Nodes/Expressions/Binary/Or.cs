namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;
/// <summary>
/// Represents a binary expression that returns true if either child is truthy (short circuits)
/// </summary>
/// <seealso cref="Value.Truthy"/>
public class Or : Binary
{
    internal Or(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    // This should only be called if the left hand side is falsy
    internal override Value GetResult(Value leftHandSide, Value rightHandSide) => rightHandSide.Truthy;

    internal override bool ShortCircuitOn(Value value) => value.Truthy;

    internal override Value ShortCircuitValue => true;
}