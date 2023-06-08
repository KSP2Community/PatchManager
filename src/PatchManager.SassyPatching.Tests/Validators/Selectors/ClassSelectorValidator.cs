namespace PatchManager.SassyPatching.Tests.Validators.Selectors;

public class ClassSelectorValidator : ParseValidator<ClassSelector>
{
    public string ClassName = "";
    public override bool Validate(ClassSelector node) => node.ClassName == ClassName;
}