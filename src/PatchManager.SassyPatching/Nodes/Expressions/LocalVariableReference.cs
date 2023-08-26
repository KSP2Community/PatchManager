using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents a local variable (indexing into $current)
/// </summary>
public class LocalVariableReference : Expression
{
    /// <summary>
    /// The local variable $current[...]
    /// </summary>
    public readonly string LocalVariableName;
    internal LocalVariableReference(Coordinate c, string localVariableName) : base(c)
    {
        LocalVariableName = localVariableName;
    }

    /// <inheritdoc />
    public override DataValue Compute(Environment environment)
    {
        var current = environment["current"].Dictionary;
        return current.TryGetValue(LocalVariableName, out var compute) ? compute : new DataValue(DataValue.DataType.None);
    }
}