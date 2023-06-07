namespace PatchManager.SassyPatching.Nodes;

public class Block : Node
{
    public List<Node> Children;
    public Block(Coordinate c, List<Node> children) : base(c)
    {
        Children = children;
    }
}