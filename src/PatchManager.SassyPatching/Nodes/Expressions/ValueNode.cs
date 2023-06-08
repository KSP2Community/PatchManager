namespace PatchManager.SassyPatching.Nodes.Expressions;

public class ValueNode : Expression
{
    public Value StoredValue;
    public ValueNode(Coordinate c, Value storedValue) : base(c)
    {
        StoredValue = storedValue;
    }

    public override Value Compute(Environment environment) => StoredValue;
}