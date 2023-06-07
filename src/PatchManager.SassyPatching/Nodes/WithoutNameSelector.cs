namespace PatchManager.SassyPatching.Nodes;

public class WithoutNameSelector : Selector
{
    public string NamePattern;
    public WithoutNameSelector(Coordinate c, string namePattern) : base(c)
    {
        NamePattern = namePattern;
    }
}