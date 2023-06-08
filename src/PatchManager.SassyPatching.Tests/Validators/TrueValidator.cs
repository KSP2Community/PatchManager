namespace PatchManager.SassyPatching.Tests.Validators;

public class TrueValidator : ParseValidator
{
    public override bool Validate(Node node) => true;
}