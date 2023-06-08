namespace PatchManager.SassyPatching.Tests.Validators.Statements.TopLevel;

public class StageDefinitionValidator : ParseValidator<StageDefinition>
{
    public string StageName = "";
    public ulong StagePriority = 0;
    public override bool Validate(StageDefinition node)
    {
        return node.StageName == StageName && node.StagePriority == StagePriority;
    }
}