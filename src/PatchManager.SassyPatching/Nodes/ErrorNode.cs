namespace PatchManager.SassyPatching.Nodes;

/// <summary>
/// A node representing a transformation error
/// </summary>
public class ErrorNode : Node
{
    /// <summary>
    /// The error message
    /// </summary>
    public string Error;
    internal ErrorNode(Coordinate c, string error) : base(c)
    {
        Error = error;
    }
}