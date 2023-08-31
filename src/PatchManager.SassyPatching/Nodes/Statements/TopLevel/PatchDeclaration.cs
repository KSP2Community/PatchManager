using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

/// <summary>
/// Represents a declaration of which labels to patch
/// </summary>
public class PatchDeclaration : Node
{
    /// <summary>
    /// The labels to patch
    /// </summary>
    public List<string> Labels;

    internal PatchDeclaration(Coordinate c, List<string> labels) : base(c) => Labels = labels;

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        environment.GlobalEnvironment.Universe.PatchLabels(Labels.ToArray());
    }
}