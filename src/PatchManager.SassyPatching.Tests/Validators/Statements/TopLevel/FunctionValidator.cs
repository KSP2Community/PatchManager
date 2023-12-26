namespace PatchManager.SassyPatching.Tests.Validators.Statements.TopLevel;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="Function"/>
/// </summary>
public class FunctionValidator : ParseValidator<Function>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="Function"/>
    /// </summary>
    public string Name = "";
    
    /// <summary>
    /// A list of validators used to match against the corresponding list of nodes in a value of type <see cref="Function"/> 
    /// </summary>
    public List<ArgumentValidator> Arguments = new();

    /// <summary>
    /// A list of validators used to match against the corresponding list of nodes in a value of type <see cref="Function"/> 
    /// </summary>
    public List<ParseValidator> Body = new();


    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(Function node)
    {
        if (node.Name != Name) return false;
        if (Arguments.Count != node.Arguments.Count) return false;
        if (Arguments.Where((validator, idx) => !validator.Validate(node.Arguments[idx])).Any()) return false;
        if (Body.Count != node.Body.Count) return false;
        return !Body.Where((validator, idx) => !validator.Validate(node.Body[idx])).Any();
    }
}