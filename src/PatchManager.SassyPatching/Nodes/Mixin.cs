namespace PatchManager.SassyPatching.Nodes;

public class Mixin : Node
{
    public string Name;
    public List<Argument> Arguments;
    public List<Node> Body;
    public Mixin(Coordinate c, string name, List<Argument> arguments, List<Node> body) : base(c)
    {
        Name = name;
        Arguments = arguments;
        Body = body;
    }
}