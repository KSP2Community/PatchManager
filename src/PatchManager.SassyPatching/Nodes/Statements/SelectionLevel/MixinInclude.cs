namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// Represents a mixin inclusion
/// </summary>
public class MixinInclude : Node
{
    /// <summary>
    /// The name of the mixin being included
    /// </summary>
    public readonly string MixinName;
    /// <summary>
    /// The list of arguments to the mixin being included
    /// </summary>
    public readonly List<CallArgument> Arguments;

    internal MixinInclude(Coordinate c, string mixinName, List<CallArgument> arguments) : base(c)
    {
        MixinName = mixinName;
        Arguments = arguments;
    }
}