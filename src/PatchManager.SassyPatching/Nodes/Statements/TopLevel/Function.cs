namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

public class Function : Node
{
    public string Name;
    public List<Argument> Arguments;
    public List<Node> Body;
    public Function(Coordinate c, string name, List<Argument> arguments, List<Node> body) : base(c)
    {
        Name = name;
        Arguments = arguments;
        Body = body;
    }
}