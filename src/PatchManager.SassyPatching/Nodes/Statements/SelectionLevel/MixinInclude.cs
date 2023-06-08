namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

public class MixinInclude : Node
{
    public string MixinName;
    public List<CallArgument> Arguments;

    public MixinInclude(Coordinate c, string mixinName, List<CallArgument> arguments) : base(c)
    {
        MixinName = mixinName;
        Arguments = arguments;
    }
}