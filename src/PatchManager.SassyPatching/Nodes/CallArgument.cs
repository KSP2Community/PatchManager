using JetBrains.Annotations;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Nodes.Expressions;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes;

/// <summary>
/// Represents an argument in a function call
/// </summary>
public class CallArgument : Node
{
    /// <summary>
    /// The name of the argument, if it is a named argument
    /// </summary>
    [CanBeNull] public readonly string ArgumentName;
    /// <summary>
    /// The value passed to the function/mixin being called/instantiated
    /// </summary>
    public readonly Expression ArgumentValue;

    /// <summary>
    /// Creates a named call argument
    /// </summary>
    /// <param name="c">The location of the argument</param>
    /// <param name="argumentName">The name of the argument</param>
    /// <param name="argumentValue">The value being passed</param>
    public CallArgument(Coordinate c, [CanBeNull] string argumentName, Expression argumentValue) : base(c)
    {
        ArgumentName = argumentName;
        ArgumentValue = argumentValue;
    }
    
    internal CallArgument(Coordinate c, Expression argumentValue) : base(c)
    {
        ArgumentName = null;
        ArgumentValue = argumentValue;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
    }

    public PatchArgument Compute(Environment environment)
    {
        return new PatchArgument
        {
            ArgumentName = ArgumentName,
            ArgumentDataValue = ArgumentValue.Compute(environment)
        };
    }
}