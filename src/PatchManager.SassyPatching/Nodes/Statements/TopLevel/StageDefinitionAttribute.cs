using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

public class StageDefinitionAttribute : Node
{
    public string Relative;
    public bool After;
    public StageDefinitionAttribute(Coordinate c, string relative, bool after) : base(c)
    {
        Relative = relative;
        After = after;
    }
    public override void ExecuteIn(Environment environment)
    {
    }
}