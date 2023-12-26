using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

public class ConfigCreation : Node
{
    public string ConfigLabel;
    public string ConfigName;
    public Expression ConfigValue;
    public ConfigCreation(Coordinate c, string configLabel, string configName, Expression configValue) : base(c)
    {
        ConfigLabel = configLabel;
        ConfigName = configName;
        ConfigValue = configValue;
    }
    public override void ExecuteIn(Environment environment)
    {
        var universe = environment.GlobalEnvironment.Universe;
        if (!universe.Configs.TryGetValue(ConfigLabel, out var label))
            label = universe.Configs[ConfigLabel] = new Dictionary<string, DataValue>();
        label[ConfigName] = ConfigValue.Compute(environment);
    }
}