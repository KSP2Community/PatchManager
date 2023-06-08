namespace PatchManager.SassyPatching.Tests.Validators.Attributes;

public class RequireModAttributeValidator : ParseValidator<RequireModAttribute>
{
    public string Guid = "";
    public override bool Validate(RequireModAttribute node)
    {
        return node.Guid == Guid;
    }
}