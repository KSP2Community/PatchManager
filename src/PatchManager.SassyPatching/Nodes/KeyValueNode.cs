using PatchManager.SassyPatching.Nodes.Expressions;

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

    /// <summary>
    /// Creates a new object field declaration node
    /// </summary>
    /// <param name="c">The location of the node</param>
    /// <param name="key">The field name</param>
    /// <param name="value">The fields value</param>
    public KeyValueNode(Coordinate c, string key, Expression value) : base(c)
    {
        Key = key;
        Value = value;
    }
}