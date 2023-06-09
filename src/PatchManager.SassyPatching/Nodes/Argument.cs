using JetBrains.Annotations;

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
    [CanBeNull] public readonly Node Value;
    
    internal Argument(Coordinate c, string name, [CanBeNull] Node value = null) : base(c)
    {
        Name = name;
        Value = value;
    }
}