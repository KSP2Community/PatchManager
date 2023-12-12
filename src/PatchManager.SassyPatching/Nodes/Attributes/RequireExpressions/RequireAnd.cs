namespace PatchManager.SassyPatching.Nodes.Attributes.RequireExpressions;

public class RequireAnd : RequireExpression
{
    public RequireExpression LeftHandSide;
    public RequireExpression RightHandSide;

    public RequireAnd(Coordinate c, RequireExpression lhs, RequireExpression rhs) : base(c)
    {
        LeftHandSide = lhs;
        RightHandSide = rhs;
    }

    public override bool Execute(IReadOnlyCollection<string> loadedMods) =>
        LeftHandSide.Execute(loadedMods) && RightHandSide.Execute(loadedMods);
}