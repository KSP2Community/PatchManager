namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents a literal value
/// </summary>
public class ValueNode : Expression
{
    /// <summary>
    /// The literal value
    /// </summary>
    public readonly Value StoredValue;
    internal ValueNode(Coordinate c, Value storedValue) : base(c)
    {
        StoredValue = storedValue;
    }

    /// <inheritdoc />
    public override Value Compute(Environment environment) => StoredValue;
}