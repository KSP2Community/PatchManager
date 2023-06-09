namespace PatchManager.SassyPatching.Tests.Validators;

/// <summary>
/// Describes a validator for matching nodes of type <see cref="KeyValueNode"/>
/// </summary>
public class KeyValueValidator : ParseValidator<KeyValueNode>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="KeyValueNode"/>
    /// </summary>
    public string Key = "";
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="KeyValueNode"/>
    /// </summary>
    public ParseValidator Value = new FalseValidator();
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(KeyValueNode node) => node.Key == Key && Value.Validate(node.Value);
}