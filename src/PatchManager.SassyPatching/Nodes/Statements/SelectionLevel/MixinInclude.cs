using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// Represents a mixin inclusion
/// </summary>
public class MixinInclude : Node, ISelectionAction
{
    /// <summary>
    /// The name of the mixin being included
    /// </summary>
    public readonly string MixinName;
    /// <summary>
    /// The list of arguments to the mixin being included
    /// </summary>
    public readonly List<CallArgument> Arguments;

    internal MixinInclude(Coordinate c, string mixinName, List<CallArgument> arguments) : base(c)
    {
        MixinName = mixinName;
        Arguments = arguments;
    }

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
    }

    /// <inheritdoc />
    public void ExecuteOn(Environment environment, ISelectable selectable, IModifiable modifiable)
    {
        if (environment.GlobalEnvironment.AllMixins.TryGetValue(MixinName, out var mixin))
        {
            try
            {
                mixin.Include(environment,Arguments.Select(x => x.Compute(environment)).ToList(),selectable,modifiable);
            }
            catch (InvocationException e)
            {
                throw new InterpreterException(Coordinate, e.ToString());
            }
        }
        else
        {
            throw new InterpreterException(Coordinate, $"{MixinName} is not a valid mixin");
        }
    }
}