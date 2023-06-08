namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

public class StageDefinition : Node
{
    public string StageName;
    public ulong StagePriority;

    public StageDefinition(Coordinate c, string stageName, ulong stagePriority) : base(c)
    {
        StageName = stageName;
        StagePriority = stagePriority;
    }
}