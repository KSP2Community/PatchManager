using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.FunctionLevel;

/// <summary>
/// Represents a loop that iterates while a condition remains true
/// </summary>
public class While : Node
{
    /// <summary>
    /// The condition to test
    /// </summary>
    public readonly Expression Condition;
    /// <summary>
    /// The body to iterate
    /// </summary>
    public readonly List<Node> Children;
    internal While(Coordinate c, Expression condition, List<Node> children) : base(c)
    {
        Condition = condition;
        Children = children;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        while (Condition.Compute(environment).Truthy)
        {
            foreach (var child in Children)
            {
                child.ExecuteIn(environment);
            }
        }
    }
}