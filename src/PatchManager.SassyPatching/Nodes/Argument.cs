using JetBrains.Annotations;
using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes;

/// <summary>
/// Represents an argument definition for a function/mixin
/// </summary>
public class Argument : Node
{
    /// <summary>
    /// The name of the argument being defined
    /// </summary>
    public readonly string Name;
    /// <summary>
    /// The default value of the argument if there is one
    /// </summary>
    [CanBeNull] public readonly Expression Value;
    
    internal Argument(Coordinate c, string name, [CanBeNull] Expression value = null) : base(c)
    {
        Name = name;
        Value = value;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
    }
}