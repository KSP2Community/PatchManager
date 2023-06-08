namespace PatchManager.SassyPatching.Tests.Validators.Statements;

public class SelectionBlockValidator : ParseValidator<SelectionBlock>
{
    public List<ParseValidator> Attributes = new();
    public ParseValidator Selector = new FalseValidator();
    public List<ParseValidator> Actions = new();
    public override bool Validate(SelectionBlock node)
    {
        if (Attributes.Count != node.Attributes.Count) return false;
        if (Attributes.Where((x, y) => !x.Validate(node.Attributes[y])).Any()) return false;
        if (!Selector.Validate(node.Selector)) return false;
        if (Actions.Count != node.Actions.Count) return false;
        return !Actions.Where((x, y) => !x.Validate(node.Actions[y])).Any();
    }
}