using System.Collections;

namespace PatchManager.SassyPatching.Tests.Validators.Expressions;

public class ObjectValidator : ParseValidator<ObjectNode>, IEnumerable<KeyValueValidator>
{
    public List<KeyValueValidator> Initializers = new();
    public override bool Validate(ObjectNode node)
    {
        if (node.Initializers.Count != Initializers.Count) return false;
        return !Initializers.Where((x, y) => x.Validate(node.Initializers[y])).Any();
    }

    public IEnumerator<KeyValueValidator> GetEnumerator()
    {
        return Initializers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValueValidator initializer)
    {
        Initializers.Add(initializer);
    }
}