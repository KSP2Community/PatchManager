namespace PatchManager.SassyPatching.Tests.Validators.Selectors;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="WildcardSelector"/>
/// </summary>
public class WildcardSelectorValidator : ParseValidator<WildcardSelector>
{
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(WildcardSelector node) => true;
}