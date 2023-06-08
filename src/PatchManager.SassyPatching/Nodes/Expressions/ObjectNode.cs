namespace PatchManager.SassyPatching.Nodes.Expressions;

public class ObjectNode : Expression
{
    public Dictionary<string, Expression> Initializers;
    public ObjectNode(Coordinate c, Dictionary<string, Expression> initializers) : base(c)
    {
        Initializers = initializers;
    }

    public override Value Compute(Environment environment)
    {
        return Initializers.ToDictionary(x => x.Key, x => x.Value.Compute(environment));
    }
}