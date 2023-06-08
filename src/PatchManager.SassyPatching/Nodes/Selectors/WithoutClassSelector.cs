namespace PatchManager.SassyPatching.Nodes.Selectors;

public class WithoutClassSelector : Selector
{
    public string ClassName;
    public WithoutClassSelector(Coordinate c, string className) : base(c)
    {
        ClassName = className;
    }
}