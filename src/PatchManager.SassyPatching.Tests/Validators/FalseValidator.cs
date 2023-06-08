namespace PatchManager.SassyPatching.Tests.Validators;

public class FalseValidator : ParseValidator
{
    public override bool Validate(Node node) => false;
}