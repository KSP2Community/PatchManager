namespace PatchManager.SassyPatching.Tests.Validators.Statements.FunctionLevel;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="Return"/>
/// </summary>
public class ReturnValidator : ParseValidator<Return>
{
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="Return"/>
    /// </summary>
    public ParseValidator ReturnedValue = new FalseValidator();
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(Return node)
    {
        return ReturnedValue.Validate(node.ReturnedValue);
    }
}