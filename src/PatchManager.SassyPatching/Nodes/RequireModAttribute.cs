namespace PatchManager.SassyPatching.Nodes;

public class RequireModAttribute : SelectorAttribute
{
    public string Guid;
    public RequireModAttribute(Coordinate c, string guid) : base(c)
    {
        Guid = guid;
    }
}