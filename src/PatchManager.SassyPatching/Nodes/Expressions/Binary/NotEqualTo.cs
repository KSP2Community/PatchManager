namespace PatchManager.SassyPatching.Nodes.Expressions.Binary;

public class NotEqualTo : Binary
{
    public NotEqualTo(Coordinate c, Expression leftHandSide, Expression rightHandSide) : base(c, leftHandSide, rightHandSide)
    {
    }
    private bool ListCompare(List<Value> leftHandSide, List<Value> rightHandSide)
    {
        if (leftHandSide.Count != rightHandSide.Count)
        {
            return true;
        }
        return leftHandSide.Where((t, index) => GetResult(t, rightHandSide[index]).Boolean).Any();
    }

    private bool DictionaryCompare(Dictionary<string, Value> leftHandSide, Dictionary<string, Value> rightHandSide)
    {
        if (leftHandSide.Count != rightHandSide.Count)
        {
            return true;
        }

        foreach (var kv in leftHandSide)
        {
            if (rightHandSide.TryGetValue(kv.Key, out var rvalue))
            {
                if (GetResult(kv.Value, rvalue).Boolean)
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        return false;
    }
    public override Value GetResult(Value leftHandSide, Value rightHandSide)
    {
        if (leftHandSide.Type != rightHandSide.Type) return true;
        
        if (leftHandSide.IsBoolean)
        {
            return leftHandSide.Boolean != rightHandSide.Boolean;
        }

        if (leftHandSide.IsNumber)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return leftHandSide.Number != rightHandSide.Number;
        }

        if (leftHandSide.IsString)
        {
            return leftHandSide.String != rightHandSide.String;
        }

        if (leftHandSide.IsList)
        {
            return ListCompare(leftHandSide.List,rightHandSide.List);
        }

        if (leftHandSide.IsDictionary)
        {
            return DictionaryCompare(leftHandSide.Dictionary, rightHandSide.Dictionary);
        }
        
        return leftHandSide.IsNone;
    }

    public override bool ShortCircuitOn(Value value) => false;

    public override Value ShortCircuitValue => null;
}