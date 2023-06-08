namespace PatchManager.SassyPatching.Tests.Validators.Statements.SelectionLevel;

public class DeleteValueValidator : ParseValidator<DeleteValue>
{
    public override bool Validate(DeleteValue node) => true;
}