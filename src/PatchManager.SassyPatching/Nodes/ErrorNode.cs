namespace PatchManager.SassyPatching.Nodes;

public class ErrorNode : Node
{
    public string Error;

    public ErrorNode(Coordinate c, string error) : base(c)
    {
        Error = error;
    }
}