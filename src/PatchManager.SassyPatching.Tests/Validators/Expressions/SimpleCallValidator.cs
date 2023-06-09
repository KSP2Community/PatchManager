namespace PatchManager.SassyPatching.Tests.Validators.Expressions;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="SimpleCall"/>
/// </summary>
public class SimpleCallValidator : ParseValidator<SimpleCall>
{    
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="SimpleCall"/>
    /// </summary>
    public string FunctionName = "";
    /// <summary>
    /// A list of validators used to match against the corresponding list of nodes in a value of type <see cref="SimpleCall"/> 
    /// </summary>
    public List<ParseValidator> Arguments = new();

    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(SimpleCall node)
    {
        if (node.FunctionName != FunctionName) return false;
        if (Arguments.Count != node.Arguments.Count) return false;
        return !Arguments.Where((x, y) => !x.Validate(node.Arguments[y])).Any();
    }
}