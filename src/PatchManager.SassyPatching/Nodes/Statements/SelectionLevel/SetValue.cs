using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// Represents a value setting selection action
/// </summary>
public class SetValue : Node
{
    /// <summary>
    /// The value to set the selection to
    /// </summary>
    public readonly Expression Value;
    internal SetValue(Coordinate c, Expression value) : base(c)
    {
        Value = value;
    }
}