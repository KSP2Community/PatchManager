namespace PatchManager.SassyPatching.Nodes.Selectors;

public class NameSelector : Selector
{
    public string NamePattern;
    public NameSelector(Coordinate c, string namePattern) : base(c)
    {
        NamePattern = namePattern;
    }
}