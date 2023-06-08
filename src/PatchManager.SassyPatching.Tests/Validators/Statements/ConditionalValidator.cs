namespace PatchManager.SassyPatching.Tests.Validators.Statements;

public class ConditionalValidator : ParseValidator<Conditional>
{
    public ParseValidator Condition;
    public List<ParseValidator> Body;
    public ParseValidator? Else;
    public override bool Validate(Conditional node)
    {
        if (!Condition.Validate(node.Condition)) return false;
        if (Body.Count != node.Body.Count) return false;
        if (Body.Where((x, y) => !x.Validate(node.Body[y])).Any()) return false;
        if (Else == null && node.Else == null) return true;
        if (Else == null && node.Else != null) return false;
        if (Else != null && node.Else == null) return false;
        return Else!.Validate(node.Else!);
    }
}