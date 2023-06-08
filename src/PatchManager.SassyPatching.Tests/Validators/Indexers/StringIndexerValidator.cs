namespace PatchManager.SassyPatching.Tests.Validators.Indexers;

public class StringIndexerValidator : ParseValidator<StringIndexer>
{
    public string Index;
    public override bool Validate(StringIndexer node) => node.Index == Index;
}