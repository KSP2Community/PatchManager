namespace PatchManager.SassyPatching.Tests.Validators;

public class KeyValueValidator : ParseValidator<KeyValueNode>
{
    public string Key = "";
    public ParseValidator Value = new FalseValidator();
    public override bool Validate(KeyValueNode node) => node.Key == Key && Value.Validate(node.Value);
}