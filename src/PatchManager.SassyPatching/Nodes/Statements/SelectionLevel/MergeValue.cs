using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// Represents a merge selection action
/// </summary>
public class MergeValue : Node
{
    /// <summary>
    /// The value the selection is going to be merged with, should evaluate to a dictionary
    /// </summary>
    public readonly Expression Value;
    internal MergeValue(Coordinate c, Expression value) : base(c)
    {
        Value = value;
    }
}