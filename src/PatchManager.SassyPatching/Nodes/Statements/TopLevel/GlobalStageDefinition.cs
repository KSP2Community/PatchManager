namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;
using Environment = PatchManager.SassyPatching.Execution.Environment;

/// <summary>
/// Represents a global stage definition
/// </summary>
public class GlobalStageDefinition : Node
{
    /// <summary>
    /// The name of the stage being defined
    /// </summary>
    public readonly string StageName;
    /// <summary>
    /// The priority of the stage
    /// </summary>
    public readonly ulong StagePriority;

    internal GlobalStageDefinition(Coordinate c, string stageName, ulong stagePriority) : base(c)
    {
        StageName = stageName;
        StagePriority = stagePriority;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
    }
}