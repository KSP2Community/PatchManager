using JetBrains.Annotations;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.TopLevel;

public class ConfigUpdate : Node
{
    public Expression Priority;
    public String Label;
    [CanBeNull] public String Name;
    public Expression UpdateExpression;
    public ConfigUpdate(Coordinate c, Expression priority, string label, [CanBeNull] string name, Expression updateExpression) : base(c)
    {
        Priority = priority;
        Label = label;
        Name = name;
        UpdateExpression = updateExpression;
    }
    public override void ExecuteIn(Environment environment)
    {
        var universe = environment.GlobalEnvironment.Universe;
        var priority = Priority.Compute(environment);
        if (!priority.IsInteger)
        {
            throw new InterpreterException(Coordinate, "The priority for a config update must be an integer");
        }
        universe.AddConfigUpdater(priority.Integer,Label,Name,UpdateExpression,environment.Snapshot());
    }
}