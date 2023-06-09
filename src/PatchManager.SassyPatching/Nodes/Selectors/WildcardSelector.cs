namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selection action that matches all selectables
/// </summary>
public class WildcardSelector : Selector
{
    internal WildcardSelector(Coordinate c) : base(c)
    {
    }
}