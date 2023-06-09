using System.Collections;

namespace PatchManager.SassyPatching.Tests.Validators.Selectors;

/// <summary>
/// Describes a validator for matching nodes of type <see cref="CombinationSelector"/>
/// </summary>
public class CombinationSelectorValidator : ParseValidator<CombinationSelector>, IEnumerable<ParseValidator>
{
    private readonly List<ParseValidator> _validators = new();
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(CombinationSelector node)
    {
        if (_validators.Count != node.Selectors.Count) return false;
        return !_validators.Where((x, y) => !x.Validate(node.Selectors[y])).Any();
    }

    /// <summary>
    /// Gets an enumerator that enumerates over the validators contained in this validator
    /// </summary>
    /// <returns>The enumerator that enumerates over the validators contained in this validator</returns>
    public IEnumerator<ParseValidator> GetEnumerator()
    {
        return _validators.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Adds a validator to the children of the tree that this validator describes
    /// </summary>
    /// <param name="validator">The child validator that should be added to the end as a child of this validator</param>
    public void Add(ParseValidator validator) => _validators.Add(validator);
}