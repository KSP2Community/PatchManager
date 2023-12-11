namespace PatchManager.SassyPatching.Nodes.Attributes;

public class RunAfterStageAttribute : SelectorAttribute
{
    /// <summary>
    /// The stage that the attributed selection block will run at
    /// </summary>
    public readonly string Stage;
    internal RunAfterStageAttribute(Coordinate c, string stage) : base(c)
    {
        Stage = stage;
    }
}