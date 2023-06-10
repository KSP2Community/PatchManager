namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

/// <summary>
/// Represents a binary expression that returns true if both children are truthy (short circuits)
/// </summary>
/// <seealso cref="DataValue.Truthy"/>
public class And : Binary
{
    public And(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide) => rightHandSide.Truthy;

    internal override bool ShortCircuitOn(DataValue dataValue) => !dataValue.Truthy;

    internal override DataValue ShortCircuitDataValue => false;
}