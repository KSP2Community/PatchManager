using PatchManager.SassyPatching.Execptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class Add : Binary
{
    public Add(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    public override Value GetResult(Value leftHandSide, Value rightHandSide)
    {
        switch (leftHandSide.Type)
        {
            case Value.ValueType.Number when rightHandSide.IsNumber:
                return leftHandSide.Number + rightHandSide.Number;
            case Value.ValueType.String when rightHandSide.IsString:
                return leftHandSide.String + rightHandSide.String;
            case Value.ValueType.List when rightHandSide.IsList:
            {
                // If every value is immutable a shallow copy should be fine
                var newList = new List<Value>(leftHandSide.List);
                newList.AddRange(rightHandSide.List);
                return newList;
            }
            case Value.ValueType.None:
            case Value.ValueType.Boolean:
            case Value.ValueType.Dictionary:
            case Value.ValueType.Deletion:
            default:
                throw new BinaryExpressionTypeException("add", leftHandSide.Type.ToString(), rightHandSide.Type.ToString());
        }
    }

    public override bool ShortCircuitOn(Value value) => false;
    public override Value ShortCircuitValue => null;
}