namespace PatchManager.SassyPatching.Nodes;

public class RulesetSelector : Selector
{
    public string RulesetName;
    public RulesetSelector(Coordinate c, string rulesetName) : base(c)
    {
        RulesetName = rulesetName;
    }
}