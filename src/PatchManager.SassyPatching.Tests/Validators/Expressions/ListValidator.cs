using System.Collections;

namespace PatchManager.SassyPatching.Tests.Validators.Expressions;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="ListNode"/>
/// </summary>
public class ListValidator : ParseValidator<ListNode>, IEnumerable<ParseValidator>
{
    private readonly List<ParseValidator> _expressions = new();
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(ListNode node)
    {
        if (node.Expressions.Count != _expressions.Count) return false;
        return !_expressions.Where((x, y) => !x.Validate(node.Expressions[y])).Any();
    }

    /// <summary>
    /// Gets an enumerator that enumerates over the validators contained in this validator
    /// </summary>
    /// <returns>The enumerator that enumerates over the validators contained in this validator</returns>
    public IEnumerator<ParseValidator> GetEnumerator()
    {
        return _expressions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Adds a validator to the children of the tree that this validator describes
    /// </summary>
    /// <param name="validator">The child validator that should be added to the end as a child of this validator</param>
    public void Add(ParseValidator validator)
    {
        _expressions.Add(validator);
    }
}