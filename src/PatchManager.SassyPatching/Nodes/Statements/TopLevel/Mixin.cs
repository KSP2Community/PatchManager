using PatchManager.SassyPatching.Execution;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

/// <summary>
/// Represents a selection block mixin
/// </summary>
public class Mixin : Node
{
    /// <summary>
    /// The name of the mixin
    /// </summary>
    public readonly string Name;
    /// <summary>
    /// The arguments that the mixin takes
    /// </summary>
    public readonly List<Argument> Arguments;
    /// <summary>
    /// The list of nodes to be "mixed in" when included
    /// </summary>
    public readonly List<Node> Body;
    internal Mixin(Coordinate c, string name, List<Argument> arguments, List<Node> body) : base(c)
    {
        Name = name;
        Arguments = arguments;
        Body = body;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        environment.GlobalEnvironment.AllMixins[Name] = new PatchMixin(this);
    }
}