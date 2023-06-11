using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Expressions;

/// <summary>
/// Represents an expression node
/// </summary>
public abstract class Expression : Node
{

    /// <summary>
    /// Computes the value of the expression in the given environment
    /// </summary>
    /// <param name="environment">The environment to compute the expression within</param>
    /// <returns>The computed value</returns>
    /// <exception cref="Exceptions.InterpreterException">Thrown if any error happens</exception>
    public abstract DataValue Compute(Environment environment);
    internal Expression(Coordinate c) : base(c)
    {
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        _ = Compute(environment);
    }
}