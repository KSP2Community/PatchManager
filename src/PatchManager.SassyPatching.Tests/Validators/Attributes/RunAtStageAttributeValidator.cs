namespace PatchManager.SassyPatching.Tests.Validators.Attributes;

public class RunAtStageAttributeValidator : ParseValidator<RunAtStageAttribute>
{
    public string Stage = "";
    public override bool Validate(RunAtStageAttribute node) => node.Stage == Stage;
}