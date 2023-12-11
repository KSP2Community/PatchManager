namespace PatchManager.SassyPatching.Nodes.Attributes;

public class RunBeforeStageAttribute : SelectorAttribute
{
    /// <summary>
    /// The stage that the attributed selection block will run at
    /// </summary>
    public readonly string Stage;
    internal RunBeforeStageAttribute(Coordinate c, string stage) : base(c)
    {
        Stage = stage;
    }
}