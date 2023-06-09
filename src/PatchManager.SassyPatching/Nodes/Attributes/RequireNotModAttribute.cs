namespace PatchManager.SassyPatching.Nodes.Attributes;

/// <summary>
/// Represents an attribute that modifies a selection block to only run if a mod is not loaded
/// </summary>
public class RequireNotModAttribute : SelectorAttribute
{
    /// <summary>
    /// The GUID of the mod
    /// </summary>
    public readonly string Guid;

    internal RequireNotModAttribute(Coordinate c, string guid) : base(c)
    {
        Guid = guid;
    }
}