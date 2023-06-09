namespace PatchManager.SassyPatching.Tests.Validators.Expressions;

public class UnaryValidator<T> : ParseValidator<T> where T : Unary
{
    public ParseValidator Child = new FalseValidator();
    public override bool Validate(T node) => Child.Validate(node.Child);
}