namespace PatchManager.SassyPatching.Tests.Validators.Expressions;

public class MemberCallValidator : ParseValidator<MemberCall>
{
    public ParseValidator LeftHandSide = new FalseValidator();
    public string FunctionName = "";
    public List<ParseValidator> Arguments = new();
    public override bool Validate(MemberCall node)
    {
        if (!LeftHandSide.Validate(node.LeftHandSide)) return false;
        if (node.FunctionName != FunctionName) return false;
        if (Arguments.Count != node.Arguments.Count) return false;
        return !Arguments.Where((x, y) => !x.Validate(node.Arguments[y])).Any();
    }
}