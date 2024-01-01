using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Attributes.RequireExpressions;

/// <summary>
/// Represents a node in a require expression
/// </summary>
public abstract class RequireExpression : Node
{
    internal RequireExpression(Coordinate c) : base(c) { }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment) { }

    public abstract bool Execute(IReadOnlyCollection<string> loadedMods, Environment e);
}