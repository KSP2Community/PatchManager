namespace PatchManager.SassyPatching.Tests.Validators;

/// <summary>
/// Describes a validator for matching nodes of type <see cref="CallArgument"/>
/// </summary>
public class CallArgumentValidator : ParseValidator<CallArgument>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="CallArgument"/>
    /// </summary>
    public string ArgumentName = null;
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="CallArgument"/>
    /// </summary>
    public ParseValidator ArgumentValue = new FalseValidator();
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(CallArgument node)
    {
        if (ArgumentName != node.ArgumentName) return false;
        return ArgumentValue.Validate(node.ArgumentValue);
    }
}