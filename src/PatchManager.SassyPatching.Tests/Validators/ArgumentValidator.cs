namespace PatchManager.SassyPatching.Tests.Validators;

/// <summary>
/// Describes a validator for matching nodes of type <see cref="Argument"/>
/// </summary>
public class ArgumentValidator : ParseValidator<Argument>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="Argument"/>
    /// </summary>
    public string Name = "";

    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="Argument"/>
    /// </summary>
    public ParseValidator Value = null;

    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(Argument node)
    {
        if (Name != node.Name) return false;
        if (Value == null && node.Value != null) return false;
        if (Value != null && node.Value == null) return false;
        if (Value == null && node.Value == null) return true;
        return Value!.Validate(node.Value!);
    }
}