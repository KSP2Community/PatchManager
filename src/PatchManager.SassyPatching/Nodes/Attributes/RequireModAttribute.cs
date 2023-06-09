namespace PatchManager.SassyPatching.Nodes.Attributes;
/// <summary>
/// Represents an attribute that modifies a selection block to only run if a mod is loaded
/// </summary>
public class RequireModAttribute : SelectorAttribute
{    /// <summary>
    /// The GUID of the mod
    /// </summary>
    public readonly string Guid;

    internal RequireModAttribute(Coordinate c, string guid) : base(c)
    {
        Guid = guid;
    }
}