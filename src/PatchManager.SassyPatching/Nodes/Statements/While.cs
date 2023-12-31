using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes.Expressions;
using PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements;

/// <summary>
/// Represents a loop that iterates while a condition remains true
/// </summary>
public class While : Node, ISelectionAction
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

    public void ExecuteOn(Environment environment, ISelectable selectable, IModifiable modifiable)
    {
        while (Condition.Compute(environment).Truthy)
        {
            foreach (var child in Children)
            {
                if (child is ISelectionAction selectionAction)
                {
                    selectionAction.ExecuteOn(environment,selectable,modifiable);
                }
                else
                {
                    child.ExecuteIn(environment);
                }
            }
        }
    }
}