namespace PatchManager.SassyPatching.Tests.Validators.Expressions;
/// <summary>
/// Describes a validator for matching nodes with a type derived from <see cref="Unary"/>
/// </summary>
/// <typeparam name="T">The unary node type that this validator will match</typeparam>
public class UnaryValidator<T> : ParseValidator<T> where T : Unary
{
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node with a type derived from <see cref="Unary"/>
    /// </summary>
    public ParseValidator Child = new FalseValidator();
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(T node) => Child.Validate(node.Child);
}