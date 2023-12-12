namespace PatchManager.SassyPatching.Nodes.Attributes.RequireExpressions;

public class RequireNot : RequireExpression
{
    public RequireExpression Child;

    public RequireNot(Coordinate c, RequireExpression child) : base(c) => Child = child;
    public override bool Execute(IReadOnlyCollection<string> loadedMods) => !Child.Execute(loadedMods);
}