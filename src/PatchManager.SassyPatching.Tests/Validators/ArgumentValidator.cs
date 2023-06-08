namespace PatchManager.SassyPatching.Tests.Validators;

public class ArgumentValidator : ParseValidator<Argument>
{
    public string Name = "";
    public ParseValidator? Value = null;
    public override bool Validate(Argument node)
    {
        if (Name != node.Name) return false;
        if (Value == null && node.Value != null) return false;
        if (Value != null && node.Value == null) return false;
        if (Value == null && node.Value == null) return true;
        return Value!.Validate(node.Value!);
    }
}