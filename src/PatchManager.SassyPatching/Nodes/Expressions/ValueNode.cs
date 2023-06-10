using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents a literal value
/// </summary>
public class ValueNode : Expression
{
    /// <summary>
    /// The literal value
    /// </summary>
    public readonly DataValue StoredDataValue;
    internal ValueNode(Coordinate c, DataValue storedDataValue) : base(c)
    {
        StoredDataValue = storedDataValue;
    }

    /// <inheritdoc />
    public override DataValue Compute(Environment environment) => StoredDataValue;
}