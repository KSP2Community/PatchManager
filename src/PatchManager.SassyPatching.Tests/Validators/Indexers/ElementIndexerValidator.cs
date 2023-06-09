namespace PatchManager.SassyPatching.Tests.Validators.Indexers;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="ElementIndexer"/>
/// </summary>
public class ElementIndexerValidator : ParseValidator<ElementIndexer>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="ElementIndexer"/>
    /// </summary>
    public string ElementName = "";
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(ElementIndexer node) => node.ElementName == ElementName;
}