namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

/// <summary>
/// Represents a stage definition
/// </summary>
public class StageDefinition : Node
{
    /// <summary>
    /// The name of the stage being defined
    /// </summary>
    public readonly string StageName;
    /// <summary>
    /// The priority of the stage
    /// </summary>
    public readonly ulong StagePriority;

    internal StageDefinition(Coordinate c, string stageName, ulong stagePriority) : base(c)
    {
        StageName = stageName;
        StagePriority = stagePriority;
    }
}