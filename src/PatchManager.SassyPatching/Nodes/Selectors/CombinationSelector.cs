namespace PatchManager.SassyPatching.Nodes.Selectors;
/// <summary>
/// Represents a selector that selects all selectables that get selected by any of the selectors contained within
/// </summary>
public class CombinationSelector : Selector
{
    /// <summary>
    /// The list of selectors to get the combination selection of
    /// </summary>
    public readonly List<Selector> Selectors;
    internal CombinationSelector(Coordinate c, Selector lhs, Selector rhs) : base(c)
    {
        Selectors = new();
        if (lhs is CombinationSelector lcs)
        {
            Selectors.AddRange(lcs.Selectors);
        }
        else
        {
            Selectors.Add(lhs);
        }

        if (rhs is CombinationSelector rcs)
        {
            Selectors.AddRange(rcs.Selectors);
        }
        else
        {
            Selectors.Add(rhs);
        }
    }
}