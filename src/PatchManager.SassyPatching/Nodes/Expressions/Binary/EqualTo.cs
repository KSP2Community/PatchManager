namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

/// <summary>
/// Represents a binary expression that returns true if both of its children are equal
/// </summary>
public class EqualTo : Binary
{
    internal EqualTo(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }

    private bool ListCompare(List<DataValue> leftHandSide, List<DataValue> rightHandSide)
    {
        if (leftHandSide.Count != rightHandSide.Count)
        {
            return false;
        }
        return !leftHandSide.Where((t, index) => !GetResult(t, rightHandSide[index]).Boolean).Any();
    }

    private bool DictionaryCompare(Dictionary<string, DataValue> leftHandSide, Dictionary<string, DataValue> rightHandSide)
    {
        if (leftHandSide.Count != rightHandSide.Count)
        {
            return false;
        }

        foreach (var kv in leftHandSide)
        {
            if (rightHandSide.TryGetValue(kv.Key, out var rvalue))
            {
                if (!GetResult(kv.Value, rvalue).Boolean)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    internal override DataValue GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {
        if (leftHandSide.Type != rightHandSide.Type) return false;
        
        if (leftHandSide.IsBoolean)
        {
            return leftHandSide.Boolean == rightHandSide.Boolean;
        }

        if (leftHandSide.IsNumber)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return leftHandSide.Number == rightHandSide.Number;
        }

        if (leftHandSide.IsString)
        {
            return leftHandSide.String == rightHandSide.String;
        }

        if (leftHandSide.IsList)
        {
            return ListCompare(leftHandSide.List,rightHandSide.List);
        }

        if (leftHandSide.IsDictionary)
        {
            return DictionaryCompare(leftHandSide.Dictionary, rightHandSide.Dictionary);
        }
        
        return true;
    }

    internal override bool ShortCircuitOn(DataValue dataValue) => false;

    internal override DataValue ShortCircuitDataValue => null;
}