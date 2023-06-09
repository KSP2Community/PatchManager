namespace PatchManager.SassyPatching.Tests.Validators.Selectors;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="ChildSelector"/>
/// </summary>
public class ChildSelectorValidator : ParseValidator<ChildSelector>
{   /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="ChildSelector"/>
    /// </summary>
    public ParseValidator Parent = new FalseValidator();
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="ChildSelector"/>
    /// </summary>
    public ParseValidator Child = new FalseValidator();
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(ChildSelector node) => Parent.Validate(node.Parent) && Child.Validate(node.Child);
}