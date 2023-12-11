using PatchManager.SassyPatching.Exceptions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

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
    /// The priority of the stage [between 0 and 999]
    /// </summary>
    public readonly ulong StagePriority;

    internal StageDefinition(Coordinate c, string stageName, ulong stagePriority) : base(c)
    {
        StageName = stageName;
        StagePriority = stagePriority;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        if (StagePriority > 999)
            throw new InterpreterException(Coordinate,
                "Stage priority outside of global stages must be between 0 and 999");
    }
}