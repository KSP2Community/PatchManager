using System.Collections;

namespace PatchManager.SassyPatching.Tests.Validators.Statements.TopLevel;

public class FunctionValidator : ParseValidator<Function>
{
    public string Name;
    
    public List<ArgumentValidator> Arguments;

    public List<ParseValidator> Body;

    public override bool Validate(Function node)
    {
        if (node.Name != Name) return false;
        if (Arguments.Count != node.Arguments.Count) return false;
        if (Arguments.Where((validator, idx) => !validator.Validate(node.Arguments[idx])).Any()) return false;
        if (Body.Count != node.Body.Count) return false;
        return !Body.Where((validator, idx) => !validator.Validate(node.Arguments[idx])).Any();
    }

    public IEnumerator<ParseValidator> GetEnumerator()
    {
        throw new NotImplementedException();
    }
}