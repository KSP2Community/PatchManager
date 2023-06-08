namespace PatchManager.SassyPatching.Nodes.Attributes;

public class RunAtStageAttribute : SelectorAttribute
{
    public string Stage;
    public RunAtStageAttribute(Coordinate c, string stage) : base(c)
    {
        Stage = stage;
    }
}