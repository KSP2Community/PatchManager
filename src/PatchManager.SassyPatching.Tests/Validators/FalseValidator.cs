namespace PatchManager.SassyPatching.Tests.Validators;

/// <summary>
/// Describes a validator that never matches
/// </summary>
public class FalseValidator : ParseValidator
{
    /// <summary>
    /// Given any node, it does not match
    /// </summary>
    /// <param name="node">Ignored</param>
    /// <returns>False</returns>
    public override bool Validate(Node node) => false;
}