using System.Collections;

namespace PatchManager.SassyPatching.Tests.Validators.Expressions;

public class ListValidator : ParseValidator<ListNode>, IEnumerable<ParseValidator>
{
    public List<ParseValidator> Expressions = new();
    public override bool Validate(ListNode node)
    {
        if (node.Expressions.Count != Expressions.Count) return false;
        return !Expressions.Where((x, y) => !x.Validate(node.Expressions[y])).Any();
    }

    public IEnumerator<ParseValidator> GetEnumerator()
    {
        return Expressions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(ParseValidator validator)
    {
        Expressions.Add(validator);
    }
}