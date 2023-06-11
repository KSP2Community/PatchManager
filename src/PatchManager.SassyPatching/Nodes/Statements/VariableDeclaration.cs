using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements;

/// <summary>
/// Represents a variable declaration
/// </summary>
public class VariableDeclaration : Node
{ 
    /// <summary>
    /// The name of the variable being declared
    /// </summary>
    public readonly string Variable;
    /// <summary>
    /// The value being assigned to the variable
    /// </summary>
    public readonly Expression Value;

    internal VariableDeclaration(Coordinate c, string variable, Expression value) : base(c)
    {
        Variable = variable;
        Value = value;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        environment[Variable] = Value.Compute(environment);
    }
}