namespace PatchManager.SassyPatching.Tests.Validators.Indexers;

public class NumberIndexerValidator : ParseValidator<NumberIndexer>
{
    public ulong Index;
    public override bool Validate(NumberIndexer node) => node.Index == Index;
}