namespace PatchManager.SassyPatching.Nodes.Expressions;

public class ObjectNode : Expression
{
    public List<KeyValueNode> Initializers;
    public ObjectNode(Coordinate c, List<KeyValueNode> initializers) : base(c)
    {
        Initializers = initializers;
    }

    public override Value Compute(Environment environment)
    {
        return Initializers.ToDictionary(x => x.Key, x => x.Value.Compute(environment));
    }
}