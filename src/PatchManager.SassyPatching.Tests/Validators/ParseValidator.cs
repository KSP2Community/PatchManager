namespace PatchManager.SassyPatching.Tests.Validators;

public abstract class ParseValidator
{
    public abstract bool Validate(Node node);
}

public abstract class ParseValidator<T> : ParseValidator where T : Node
{
    public sealed override bool Validate(Node node) => node is T tNode && Validate(tNode);

    public abstract bool Validate(T node);
}