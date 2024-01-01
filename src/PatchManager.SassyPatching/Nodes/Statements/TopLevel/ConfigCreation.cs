using PatchManager.SassyPatching.Exceptions;
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
        var configLabel = "";
        var configName = "";
        try
        {
            configLabel = ConfigLabel.Interpolate(environment);
            configName = ConfigName.Interpolate(environment);
        }
        catch (Exception e)
        {
            throw new InterpolationException(Coordinate, e.Message);
        }
        
        var universe = environment.GlobalEnvironment.Universe;
        if (!universe.Configs.TryGetValue(configLabel, out var label))
            label = universe.Configs[configLabel] = new Dictionary<string, DataValue>();
        label[configName] = ConfigValue.Compute(environment);
    }
}