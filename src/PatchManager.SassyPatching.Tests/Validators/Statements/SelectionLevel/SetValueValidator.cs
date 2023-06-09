namespace PatchManager.SassyPatching.Tests.Validators.Statements.SelectionLevel;

/// <summary>
/// Describes a validator for matching nodes of type <see cref="SetValue"/>
/// </summary>
public class SetValueValidator : ParseValidator<SetValue>
{
    
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="SetValue"/>
    /// </summary>
    public ParseValidator Value = new FalseValidator();
    

    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(SetValue node) => Value.Validate(node.Value);
}