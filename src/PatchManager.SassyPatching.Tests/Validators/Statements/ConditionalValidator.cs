namespace PatchManager.SassyPatching.Tests.Validators.Statements;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="Conditional"/>
/// </summary>
public class ConditionalValidator : ParseValidator<Conditional>
{
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="Conditional"/>
    /// </summary>
    public ParseValidator Condition = new FalseValidator();
    /// <summary>
    /// A list of validators used to match against the corresponding list of nodes in a value of type <see cref="Conditional"/>
    /// </summary>
    public List<ParseValidator> Body = new();
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="Conditional"/>
    /// </summary>
    public ParseValidator Else = null;
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(Conditional node)
    {
        if (!Condition.Validate(node.Condition)) return false;
        if (Body.Count != node.Body.Count) return false;
        if (Body.Where((x, y) => !x.Validate(node.Body[y])).Any()) return false;
        if (Else == null && node.Else == null) return true;
        if (Else == null && node.Else != null) return false;
        if (Else != null && node.Else == null) return false;
        return Else!.Validate(node.Else!);
    }
}