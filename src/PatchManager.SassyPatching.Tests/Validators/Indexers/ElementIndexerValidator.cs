namespace PatchManager.SassyPatching.Tests.Validators.Indexers;

public class ElementIndexerValidator : ParseValidator<ElementIndexer>
{
    public string ElementName = "";
    public override bool Validate(ElementIndexer node) => node.ElementName == ElementName;
}