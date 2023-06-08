using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes;

public class KeyValueNode : Node
{
    public string Key;
    public Expression Value;

    public KeyValueNode(Coordinate c, string key, Expression value) : base(c)
    {
        Key = key;
        Value = value;
    }
}