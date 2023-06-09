namespace PatchManager.SassyPatching.Nodes.Attributes;

/// <summary>
/// Represents an attribute that defines the stage of a selection block
/// </summary>
public class RunAtStageAttribute : SelectorAttribute
{
    /// <summary>
    /// The stage that the attributed selection block will run at
    /// </summary>
    public readonly string Stage;
    internal RunAtStageAttribute(Coordinate c, string stage) : base(c)
    {
        Stage = stage;
    }
}