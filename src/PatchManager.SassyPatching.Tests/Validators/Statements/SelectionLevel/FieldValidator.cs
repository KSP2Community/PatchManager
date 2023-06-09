namespace PatchManager.SassyPatching.Tests.Validators.Statements.SelectionLevel;
/// <summary>
/// Describes a validator for matching nodes of type <see cref="Field"/>
/// </summary>
public class FieldValidator : ParseValidator<Field>
{
    /// <summary>
    /// A field that is used to match against the corresponding field in a node of type <see cref="Field"/>
    /// </summary>
    public string FieldName = "";
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="Field"/>
    /// </summary>
    public ParseValidator? Indexer = null;
    /// <summary>
    /// A validator that is used to match against the corresponding node in a node of type <see cref="Field"/>
    /// </summary>
    public ParseValidator FieldValue = new FalseValidator();
    /// <summary>
    /// Determines if a node matches the tree defined by this validator
    /// </summary>
    /// <param name="node">The node to match against</param>
    /// <returns>True if the node matches against the tree defined by this validator</returns>
    public override bool Validate(Field node)
    {
        if (FieldName != node.FieldName) return false;
        if (Indexer == null && node.Indexer != null) return false;
        if (Indexer != null && node.Indexer == null) return false;
        if (Indexer != null && node.Indexer != null && !Indexer.Validate(node.Indexer)) return false;
        return FieldValue.Validate(node.FieldValue);
    }
}