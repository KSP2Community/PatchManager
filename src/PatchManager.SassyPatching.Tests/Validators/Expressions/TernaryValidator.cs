namespace PatchManager.SassyPatching.Tests.Validators.Expressions;

public class TernaryValidator : ParseValidator<Ternary>
{
    public ParseValidator LeftHandSide = new FalseValidator();
    public ParseValidator Condition = new FalseValidator();
    public ParseValidator RightHandSide = new FalseValidator();

    public override bool Validate(Ternary node) => LeftHandSide.Validate(node.LeftHandSide) &&
                                                   Condition.Validate(node.Condition) &&
                                                   RightHandSide.Validate(node.RightHandSide);
}