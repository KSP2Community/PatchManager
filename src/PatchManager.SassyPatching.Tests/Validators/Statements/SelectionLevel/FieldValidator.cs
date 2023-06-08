namespace PatchManager.SassyPatching.Tests.Validators.Statements.SelectionLevel;

public class FieldValidator : ParseValidator<Field>
{
    public string FieldName = "";
    public ParseValidator? Indexer = null;
    public ParseValidator FieldValue = new FalseValidator();
    public override bool Validate(Field node)
    {
        if (FieldName != node.FieldName) return false;
        if (Indexer == null && node.Indexer != null) return false;
        if (Indexer != null && node.Indexer == null) return false;
        if (Indexer != null && node.Indexer != null && !Indexer.Validate(node.Indexer)) return false;
        return FieldValue.Validate(node.FieldValue);
    }
}