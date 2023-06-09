namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents an object initializer
/// </summary>
public class ObjectNode : Expression
{
    /// <summary>
    /// The fields being initialized in this object
    /// </summary>
    public readonly List<KeyValueNode> Initializers;
    internal ObjectNode(Coordinate c, List<KeyValueNode> initializers) : base(c)
    {
        Initializers = initializers;
    }

    /// <inheritdoc />
    public override Value Compute(Environment environment)
    {
        return Initializers.ToDictionary(x => x.Key, x => x.Value.Compute(environment));
    }
}