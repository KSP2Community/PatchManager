using PatchManager.SassyPatching.Execution;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents a function definition
/// </summary>
public class Closure : Expression
{
    /// <summary>
    /// The list of arguments the function takes
    /// </summary>
    public readonly List<Argument> Arguments;
    /// <summary>
    /// The list of statements to be executed upon a function call
    /// </summary>
    public readonly List<Node> Body;
    internal Closure(Coordinate c, List<Argument> arguments, List<Node> body) : base(c)
    {
        Arguments = arguments;
        Body = body;
    }

    /// <inheritdoc />
    public override DataValue Compute(Environment environment)
    {
        return new SassyPatchClosure(environment.Snapshot(), this);
    }
}