namespace PatchManager.SassyPatching.Tests.Validators.Expressions;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="Ternary"/>
/// </summary>
public class TernaryValidator : ParseValidator<Ternary>
{
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="Ternary"/>
    /// </summary>
    public ParseValidator LeftHandSide = new FalseValidator();
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="Ternary"/>
    /// </summary>
    public ParseValidator Condition = new FalseValidator();
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="Ternary"/>
    /// </summary>
    public ParseValidator RightHandSide = new FalseValidator();

    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(Ternary node) => LeftHandSide.Validate(node.LeftHandSide) &&
                                                   Condition.Validate(node.Condition) &&
                                                   RightHandSide.Validate(node.RightHandSide);
}