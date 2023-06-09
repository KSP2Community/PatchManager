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
    public readonly Node Value;

    internal VariableDeclaration(Coordinate c, string variable, Node value) : base(c)
    {
        Variable = variable;
        Value = value;
    }
}