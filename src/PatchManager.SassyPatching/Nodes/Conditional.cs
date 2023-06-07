using JetBrains.Annotations;

namespace PatchManager.SassyPatching.Nodes;

public class Conditional : Node
{
    public Node Condition;
    public List<Node> Body;
    [CanBeNull] public Node Else;

    public Conditional(Coordinate c, Node condition, List<Node> body, [CanBeNull] Node @else = null) : base(c)
    {
        Condition = condition;
        Body = body;
        Else = @else;
    }
}