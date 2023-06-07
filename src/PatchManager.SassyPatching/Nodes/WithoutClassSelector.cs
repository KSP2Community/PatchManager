namespace PatchManager.SassyPatching.Nodes;

public class WithoutClassSelector : Selector
{
    public string ClassName;
    public WithoutClassSelector(Coordinate c, string className) : base(c)
    {
        ClassName = className;
    }
}