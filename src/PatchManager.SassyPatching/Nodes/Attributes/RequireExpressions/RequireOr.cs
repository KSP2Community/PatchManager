namespace PatchManager.SassyPatching.Nodes.Attributes.RequireExpressions;

public class RequireOr : RequireExpression
{
    public RequireExpression LeftHandSide;
    public RequireExpression RightHandSide;

    public RequireOr(Coordinate c, RequireExpression lhs, RequireExpression rhs) : base(c)
    {
        LeftHandSide = lhs;
        RightHandSide = rhs;
    }

    public override bool Execute(IReadOnlyCollection<string> loadedMods) =>
        LeftHandSide.Execute(loadedMods) || RightHandSide.Execute(loadedMods);
}