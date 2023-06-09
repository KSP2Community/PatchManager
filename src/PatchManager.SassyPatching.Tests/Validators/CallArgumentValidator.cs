namespace PatchManager.SassyPatching.Tests.Validators;

public class CallArgumentValidator : ParseValidator<CallArgument>
{
    public string? ArgumentName = null;
    public ParseValidator ArgumentValue = new FalseValidator();
    public override bool Validate(CallArgument node)
    {
        if (ArgumentName != node.ArgumentName) return false;
        return ArgumentValue.Validate(node.ArgumentValue);
    }
}