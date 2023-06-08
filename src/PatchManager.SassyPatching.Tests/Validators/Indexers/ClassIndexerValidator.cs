namespace PatchManager.SassyPatching.Tests.Validators.Indexers;

public class ClassIndexerValidator : ParseValidator<ClassIndexer>
{
    public string ClassName = "";
    public override bool Validate(ClassIndexer node) => node.ClassName == ClassName;
}