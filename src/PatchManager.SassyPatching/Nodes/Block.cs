using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes;

/// <summary>
/// Represents a group of nodes
/// </summary>
public class Block : Node, ISelectionAction
{
    /// <summary>
    /// The children of this block node
    /// </summary>
    public readonly List<Node> Children;
    
    internal Block(Coordinate c, List<Node> children) : base(c)
    {
        Children = children;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
        foreach (var child in Children)
        {
            child.ExecuteIn(environment);
        }
    }

    /// <inheritdoc />
    public void ExecuteOn(Environment environment, ISelectable selectable, IModifiable modifiable)
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