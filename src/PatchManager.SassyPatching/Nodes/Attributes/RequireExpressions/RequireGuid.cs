using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Attributes.RequireExpressions;

public class RequireGuid : RequireExpression
{
    public string Guid;

    public RequireGuid(Coordinate c, string guid) : base(c) => Guid = guid;

    public override bool Execute(IReadOnlyCollection<string> loadedMods, Environment e) => loadedMods.Contains(Guid.Interpolate(e));
}