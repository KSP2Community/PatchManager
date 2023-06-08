using System.Collections;

namespace PatchManager.SassyPatching.Tests.Validators;

// Used for validating a parse tree
// How do we validate a parse tree
// Well
// new TreeValidator {
//
// }
public class BlockValidator : ParseValidator<Block>, IEnumerable<ParseValidator>
{
    // ReSharper disable once CollectionNeverUpdated.Local
    private readonly List<ParseValidator> _validators = new();


    public IEnumerator<ParseValidator> GetEnumerator()
    {
        return _validators.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override bool Validate(Block patch)
    {
        if (patch.Children.Count != _validators.Count) return false;
        return !_validators.Where((t, i) => !t.Validate(patch.Children[i])).Any();
    }

    public void Add(ParseValidator parseValidator)
    {
        _validators.Add(parseValidator);
    }
}