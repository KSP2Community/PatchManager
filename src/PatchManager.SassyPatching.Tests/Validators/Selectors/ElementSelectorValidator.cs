namespace PatchManager.SassyPatching.Tests.Validators.Selectors;

public class ElementSelectorValidator : ParseValidator<ElementSelector>
{
    public string ElementName = "";
    public override bool Validate(ElementSelector node) => node.ElementName == ElementName;
}