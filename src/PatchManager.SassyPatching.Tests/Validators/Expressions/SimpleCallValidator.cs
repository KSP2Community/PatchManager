namespace PatchManager.SassyPatching.Tests.Validators.Expressions;

public class SimpleCallValidator : ParseValidator<SimpleCall>
{    
    public string FunctionName = "";
    public List<ParseValidator> Arguments = new();
    public override bool Validate(SimpleCall node)
    {
        if (node.FunctionName != FunctionName) return false;
        if (Arguments.Count != node.Arguments.Count) return false;
        return !Arguments.Where((x, y) => !x.Validate(node.Arguments[y])).Any();
    }
}