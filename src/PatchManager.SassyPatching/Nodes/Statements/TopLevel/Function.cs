using PatchManager.SassyPatching.Execution;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

/// <summary>
/// Represents a function definition
/// </summary>
public class Function : Node
{
    /// <summary>
    /// The name of the function
    /// </summary>
    public readonly string Name;
    /// <summary>
    /// The list of arguments the function takes
    /// </summary>
    public readonly List<Argument> Arguments;
    /// <summary>
    /// The list of statements to be executed upon a function call
    /// </summary>
    public readonly List<Node> Body;
    internal Function(Coordinate c, string name, List<Argument> arguments, List<Node> body) : base(c)
    {
        Name = name;
        Arguments = arguments;
        Body = body;
    }

    // Don't register functions just yet
    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        // We register this function as if its alone in its environment and cannot react to the changes of variables outside of its scope
        var snapshot = environment.Snapshot();
        environment.GlobalEnvironment.AllFunctions[Name] = new SassyPatchFunction(snapshot, this);
    }
}