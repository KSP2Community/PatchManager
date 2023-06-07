using JetBrains.Annotations;

namespace PatchManager.SassyPatching.Nodes;

public class Argument : Node
{
    public string Name;
    [CanBeNull] public Node Value;

    public Argument(Coordinate c, string name, [CanBeNull] Node value = null) : base(c)
    {
        Name = name;
        Value = value;
    }
}