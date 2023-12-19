using PatchManager.SassyPatching.Nodes.Attributes.RequireExpressions;

namespace PatchManager.SassyPatching.Nodes.Attributes;
/// <summary>
/// Represents an attribute that modifies a selection block to only run if a mod is loaded
/// </summary>
public class RequireModAttribute : SelectorAttribute
{
    public readonly RequireExpression Expression;

    internal RequireModAttribute(Coordinate c, RequireExpression expression) : base(c) => Expression = expression;
}