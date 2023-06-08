namespace PatchManager.SassyPatching.Nodes.Selectors;

public class ChildSelector : Selector
{
    public Selector Parent;
    public Selector Child;
    public ChildSelector(Coordinate c, Selector parent, Selector child) : base(c)
    {
        Parent = parent;
        Child = child;
    }
}