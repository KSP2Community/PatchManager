using PatchManager.SassyPatching.Nodes.Expressions;

namespace PatchManager.SassyPatching.Nodes.Attributes;

public class NewAttribute : SelectorAttribute
{
    public List<Expression> Arguments;
    public NewAttribute(Coordinate c, List<Expression> arguments) : base(c) => Arguments = arguments;
}