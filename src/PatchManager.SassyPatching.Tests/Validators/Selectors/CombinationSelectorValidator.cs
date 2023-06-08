using System.Collections;

namespace PatchManager.SassyPatching.Tests.Validators.Selectors;

public class CombinationSelectorValidator : ParseValidator<CombinationSelector>, IEnumerable<ParseValidator>
{
    public List<ParseValidator> Validators = new();
    public override bool Validate(CombinationSelector node)
    {
        if (Validators.Count != node.Selectors.Count) return false;
        return !Validators.Where((x, y) => !x.Validate(node.Selectors[y])).Any();
    }

    public IEnumerator<ParseValidator> GetEnumerator()
    {
        return Validators.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(ParseValidator validator) => Validators.Add(validator);
}