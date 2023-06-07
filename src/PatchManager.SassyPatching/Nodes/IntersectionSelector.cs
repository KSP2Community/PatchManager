namespace PatchManager.SassyPatching.Nodes;

public class IntersectionSelector : Selector
{
    public List<Selector> Selectors;
    public IntersectionSelector(Coordinate c, Selector lhs, Selector rhs) : base(c)
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