using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes;

/// <summary>
/// A node representing a Key/Value pair in an Objects definition
/// </summary>
public class KeyValueNode : Node
{
    /// <summary>
    /// They field to be defined
    /// </summary>
    public readonly string Key;
    /// <summary>
    /// A node representing the value of the field
    /// </summary>
    public readonly Expression Value;
    
    internal KeyValueNode(Coordinate c, string key, Expression value) : base(c)
    {
        Key = key;
        Value = value;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
    }
}