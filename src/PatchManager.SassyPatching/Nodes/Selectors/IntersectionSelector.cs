namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that selects all selectables that get selected by all of the selectors contained within
/// </summary>
public class IntersectionSelector : Selector
{
    /// <summary>
    /// The list of selectors to get the intersection selection of
    /// </summary>
    public readonly List<Selector> Selectors;
    internal IntersectionSelector(Coordinate c, Selector lhs, Selector rhs) : base(c)
    {
        Selectors = new();
        if (lhs is IntersectionSelector lis)
        {
            Selectors.AddRange(lis.Selectors);
        }
        else
        {
            Selectors.Add(lhs);
        }

        if (rhs is IntersectionSelector ris)
        {
            Selectors.AddRange(ris.Selectors);
        }
        else
        {
            Selectors.Add(rhs);
        }
    }
}