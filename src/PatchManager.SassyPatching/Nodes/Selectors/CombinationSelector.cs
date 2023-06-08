namespace PatchManager.SassyPatching.Nodes.Selectors;

public class CombinationSelector : Selector
{
    public List<Selector> Selectors;
    public CombinationSelector(Coordinate c, Selector lhs, Selector rhs) : base(c)
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