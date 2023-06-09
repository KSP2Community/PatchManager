using System.Collections;

namespace PatchManager.SassyPatching.Tests.Validators.Expressions;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="ObjectNode"/>
/// </summary>
public class ObjectValidator : ParseValidator<ObjectNode>, IEnumerable<KeyValueValidator>
{
    private readonly List<KeyValueValidator> _initializers = new();
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(ObjectNode node)
    {
        if (node.Initializers.Count != _initializers.Count) return false;
        return !_initializers.Where((x, y) => !x.Validate(node.Initializers[y])).Any();
    }

    /// <summary>
    /// Gets an enumerator that enumerates over the validators contained in this validator
    /// </summary>
    /// <returns>The enumerator that enumerates over the validators contained in this validator</returns>
    public IEnumerator<KeyValueValidator> GetEnumerator()
    {
        return _initializers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Adds a validator to the children of the tree that this validator describes
    /// </summary>
    /// <param name="initializer">The child validator that should be added to the end as a child of this validator</param>
    public void Add(KeyValueValidator initializer)
    {
        _initializers.Add(initializer);
    }
}