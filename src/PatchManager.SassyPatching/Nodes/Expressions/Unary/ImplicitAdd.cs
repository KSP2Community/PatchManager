using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Nodes.Expressions.Unary;

public class ImplicitAdd : Implicit
{
    public ImplicitAdd(Coordinate c, Expression child) : base(c, child)
    {
    }

    // There should be a value called "$value" in the current environment, a literal new environment gets made here w/ that value set
    public override Value GetResult(Value child)
    {
        // Should be unreachable
        throw new Exception("Unreachable");
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
                throw new BinaryExpressionTypeException(Coordinate,"add", leftHandSide.Type.ToString(), rightHandSide.Type.ToString());
        }
    }
    
}