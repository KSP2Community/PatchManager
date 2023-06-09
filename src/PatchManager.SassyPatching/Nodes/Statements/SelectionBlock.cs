using PatchManager.SassyPatching.Nodes.Attributes;
using PatchManager.SassyPatching.Nodes.Selectors;

namespace PatchManager.SassyPatching.Nodes.Statements;

/// <summary>
/// Represents a selection block
/// </summary>
public class SelectionBlock : Node
{
    /// <summary>
    /// The attributes applied to this selection block
    /// </summary>
    public readonly List<SelectorAttribute> Attributes;
    /// <summary>
    /// The selector that this selection block matches
    /// </summary>
    public readonly Selector Selector;
    /// <summary>
    /// The actions to be taken upon a match
    /// </summary>
    public readonly List<Node> Actions;
    
    internal SelectionBlock(Coordinate c, List<SelectorAttribute> attributes, Selector selector, List<Node> actions) : base(c)
    {
        Attributes = attributes;
        Selector = selector;
        Actions = actions;
    }
}