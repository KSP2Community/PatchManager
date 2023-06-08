namespace PatchManager.SassyPatching.Nodes.Selectors;

public class WithoutNameSelector : Selector
{
    public string NamePattern;
    public WithoutNameSelector(Coordinate c, string namePattern) : base(c)
    {
        NamePattern = namePattern;
    }
}