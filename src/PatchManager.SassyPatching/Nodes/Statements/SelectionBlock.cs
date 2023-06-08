using PatchManager.SassyPatching.Nodes.Attributes;
using PatchManager.SassyPatching.Nodes.Selectors;

namespace PatchManager.SassyPatching.Nodes.Statements;

public class SelectionBlock : Node
{
    public List<SelectorAttribute> Attributes;
    public Selector Selector;
    public List<Node> Actions;
    
    public SelectionBlock(Coordinate c, List<SelectorAttribute> attributes, Selector selector, List<Node> actions) : base(c)
    {
        Attributes = attributes;
        Selector = selector;
        Actions = actions;
    }
}