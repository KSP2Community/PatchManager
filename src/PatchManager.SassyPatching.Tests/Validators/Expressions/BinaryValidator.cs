namespace PatchManager.SassyPatching.Tests.Validators.Expressions;

public class BinaryValidator<T> : ParseValidator<T> where T : Binary
{
    public ParseValidator LeftHandSide = new FalseValidator();
    public ParseValidator RightHandSide = new FalseValidator();

    public override bool Validate(T node) =>
        LeftHandSide.Validate(node.LeftHandSide) && RightHandSide.Validate(node.RightHandSide);
}