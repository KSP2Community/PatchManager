namespace PatchManager.SassyPatching.Tests.Validators.Statements.TopLevel;

public class ImportValidator : ParseValidator<Import>
{
    public string Library = "";

    public override bool Validate(Import node)
    {
        return node.Library == Library;
    }
}