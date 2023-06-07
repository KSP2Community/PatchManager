namespace PatchManager.SassyPatching.Nodes;

public class RunAtStageAttribute : SelectorAttribute
{
    public string Stage;
    public RunAtStageAttribute(Coordinate c, string stage) : base(c)
    {
        Stage = stage;
    }
}