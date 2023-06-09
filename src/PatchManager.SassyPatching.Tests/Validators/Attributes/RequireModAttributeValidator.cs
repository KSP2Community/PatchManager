namespace PatchManager.SassyPatching.Tests.Validators.Attributes;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="RequireModAttribute"/>
/// </summary>
public class RequireModAttributeValidator : ParseValidator<RequireModAttribute>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="RequireModAttribute"/>
    /// </summary>
    public string Guid = "";
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(RequireModAttribute node)
    {
        return node.Guid == Guid;
    }
}