namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;
/// <summary>
/// Represents a binary expression that returns true if either child is truthy (short circuits)
/// </summary>
/// <seealso cref="DataValue.Truthy"/>
public class Or : Binary
{
    internal Or(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    // This should only be called if the left hand side is falsy
    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide) => rightHandSide.Truthy;

    internal override bool ShortCircuitOn(DataValue dataValue) => dataValue.Truthy;

    internal override DataValue ShortCircuitDataValue => true;
}