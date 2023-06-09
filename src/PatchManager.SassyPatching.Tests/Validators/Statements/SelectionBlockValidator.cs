namespace PatchManager.SassyPatching.Tests.Validators.Statements;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="SelectionBlock"/>
/// </summary>
public class SelectionBlockValidator : ParseValidator<SelectionBlock>
{
    /// <summary>
    /// A list of validators used to match against the corresponding list of nodes in a value of type <see cref="SelectionBlock"/> 
    /// </summary>
    public List<ParseValidator> Attributes = new();
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="SelectionBlock"/>
    /// </summary>
    public ParseValidator Selector = new FalseValidator();
    /// <summary>
    /// A list of validators used to match against the corresponding list of nodes in a value of type <see cref="SelectionBlock"/> 
    /// </summary>
    public List<ParseValidator> Actions = new();
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(SelectionBlock node)
    {
        if (Attributes.Count != node.Attributes.Count) return false;
        if (Attributes.Where((x, y) => !x.Validate(node.Attributes[y])).Any()) return false;
        if (!Selector.Validate(node.Selector)) return false;
        if (Actions.Count != node.Actions.Count) return false;
        return !Actions.Where((x, y) => !x.Validate(node.Actions[y])).Any();
    }
}