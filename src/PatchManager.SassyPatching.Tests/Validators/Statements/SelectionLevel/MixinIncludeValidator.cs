namespace PatchManager.SassyPatching.Tests.Validators.Statements.SelectionLevel;

public class MixinIncludeValidator : ParseValidator<MixinInclude>
{
    public string MixinName = "";
    public List<ParseValidator> Arguments = new();
    public override bool Validate(MixinInclude node)
    {
        if (node.MixinName != MixinName) return false;
        if (Arguments.Count != node.Arguments.Count) return false;
        return !Arguments.Where((x, y) => !x.Validate(node.Arguments[y])).Any();
    }
}