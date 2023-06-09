namespace PatchManager.SassyPatching.Tests.Validators;

/// <summary>
/// An abstract base class for validators that describe a tree structure for the results of a <see cref="PatchManager.SassyPatching.Transformer"/> transformation to be matched against for validation
/// </summary>
public abstract class ParseValidator
{
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public abstract bool Validate(Node node);
}

/// <summary>
/// A generic extension to <see cref="ParseValidator"/> where it has a specific type of <see cref="Node"/> that the tree described by the validator will match against
/// </summary>
/// <typeparam name="T">The type of <see cref="Node"/> to match against</typeparam>
public abstract class ParseValidator<T> : ParseValidator where T : Node
{
    /// <summary>
    /// An override of <see cref="ParseValidator.Validate"/> that validates whether the node is of the type that this <see cref="ParseValidator{T}"/> matches against
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node is of the type of node that this validator matches against and if the node matches against the tree defined by this validator</returns>
    public sealed override bool Validate(Node node) => node is T tNode && Validate(tNode);

    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public abstract bool Validate(T node);
}