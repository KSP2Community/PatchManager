using JetBrains.Annotations;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes.Expressions;
using PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements;

/// <summary>
/// Represents a conditional (if-else) statement
/// </summary>
public class Conditional : Node, ISelectionAction
{
    /// <summary>
    /// The condition that is being tested
    /// </summary>
    public readonly Expression Condition;
    /// <summary>
    /// The list of nodes to be executed upon a truthy result
    /// </summary>
    public readonly List<Node> Body;
    /// <summary>
    /// The node to be executed upon a falsy result, may not exist
    /// </summary>
    [CanBeNull] public readonly Node Else;

    internal Conditional(Coordinate c, Expression condition, List<Node> body, [CanBeNull] Node @else = null) : base(c)
    {
        Condition = condition;
        Body = body;
        Else = @else;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        if (Condition.Compute(environment).Truthy)
        {
            foreach (var child in Body)
            {
                child.ExecuteIn(environment);
            }
        }
        else
        {
            Else.ExecuteIn(environment);
        }
    }

    /// <inheritdoc />
    public void ExecuteOn(Environment environment, ISelectable selectable, IModifiable modifiable)
    {
        if (Condition.Compute(environment).Truthy)
        {
            foreach (var child in Body)
            {
                if (child is ISelectionAction selectionAction)
                {
                    selectionAction.ExecuteOn(environment, selectable, modifiable);
                }
                else
                {
                    child.ExecuteIn(environment);
                }
            }
        }
        else
        {
            if (Else is ISelectionAction selectionAction)
            {
                selectionAction.ExecuteOn(environment, selectable, modifiable);
            }
            else
            {
                Else?.ExecuteIn(environment);
            }
        }
    }
}