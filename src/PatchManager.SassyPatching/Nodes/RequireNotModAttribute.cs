namespace PatchManager.SassyPatching.Nodes;

public class RequireNotModAttribute : SelectorAttribute
{
    public string Guid;
    public RequireNotModAttribute(Coordinate c, string guid) : base(c)
    {
        Guid = guid;
    }
}