namespace PatchManager.SassyPatching.Tests.Validators.Attributes;

public class RequireNotModAttributeValidator : ParseValidator<RequireNotModAttribute>
{
    public string Guid = "";
    public override bool Validate(RequireNotModAttribute node) => node.Guid == Guid;
}