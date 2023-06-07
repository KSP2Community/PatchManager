namespace PatchManager.SassyPatching.Nodes;

public class SassyPatch : Node
{
    public List<Node> Children;

    public SassyPatch(Coordinate c, List<Node> children) : base(c)
    {
        Children = children;
    }
}