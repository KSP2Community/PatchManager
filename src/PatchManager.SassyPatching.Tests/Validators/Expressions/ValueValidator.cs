namespace PatchManager.SassyPatching.Tests.Validators.Expressions;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="ValueNode"/>
/// </summary>
public class ValueValidator : ParseValidator<ValueNode>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="ValueNode"/>
    /// </summary>
    public DataValue StoredDataValue = new DataValue(DataValue.DataType.None);
    
    private static bool ListCompare(List<DataValue> leftHandSide, List<DataValue> rightHandSide)
    {
        if (leftHandSide.Count != rightHandSide.Count)
        {
            return false;
        }
        return !leftHandSide.Where((t, index) => !GetResult(t, rightHandSide[index])).Any();
    }

    private static bool DictionaryCompare(Dictionary<string, DataValue> leftHandSide, Dictionary<string, DataValue> rightHandSide)
    {
        if (leftHandSide.Count != rightHandSide.Count)
        {
            return false;
        }

        foreach (var kv in leftHandSide)
        {
            if (rightHandSide.TryGetValue(kv.Key, out var rvalue))
            {
                if (!GetResult(kv.Value, rvalue))
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

    private static bool GetResult(DataValue leftHandSide, DataValue rightHandSide)
    {
        if (leftHandSide.Type != rightHandSide.Type) return false;
        
        if (leftHandSide.IsBoolean)
        {
            return leftHandSide.Boolean == rightHandSide.Boolean;
        }

        if (leftHandSide.IsReal)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return leftHandSide.Real == rightHandSide.Real;
        }

        if (leftHandSide.IsInteger)
        {
            return leftHandSide.Integer == rightHandSide.Integer;
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
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(ValueNode node)
    {
        // Now we compare equality between values
        // Good thing is there should only be a few value types
        return GetResult(StoredDataValue, node.StoredDataValue);
    }
}