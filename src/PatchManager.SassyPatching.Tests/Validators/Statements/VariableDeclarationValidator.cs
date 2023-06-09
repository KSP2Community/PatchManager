namespace PatchManager.SassyPatching.Tests.Validators.Statements;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="VariableDeclaration"/>
/// </summary>
public class VariableDeclarationValidator : ParseValidator<VariableDeclaration>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="VariableDeclaration"/>
    /// </summary>
    public string Variable = "";
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="VariableDeclaration"/>
    /// </summary>
    public ParseValidator Value = new FalseValidator();
    
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(VariableDeclaration node)
    {
        if (node.Variable != Variable) return false;
        return Value.Validate(node.Value);
    }
}