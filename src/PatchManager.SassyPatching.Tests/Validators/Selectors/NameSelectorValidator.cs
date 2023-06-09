namespace PatchManager.SassyPatching.Tests.Validators.Selectors;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="NameSelector"/>
/// </summary>
public class NameSelectorValidator : ParseValidator<NameSelector>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="NameSelector"/>
    /// </summary>
    public string NamePattern = "";
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(NameSelector node) => node.NamePattern == NamePattern;
}