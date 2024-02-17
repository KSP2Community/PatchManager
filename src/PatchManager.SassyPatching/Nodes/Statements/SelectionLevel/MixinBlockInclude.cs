using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Statements.SelectionLevel;

/// <summary>
/// Represents a mixin block include (i.e. slotting actions into a mixin)
/// </summary>
public class MixinBlockInclude : Node, ISelectionAction
{
    /// <summary>
    /// The name of the mixin being included
    /// </summary>
    public readonly string MixinName;
    /// <summary>
    /// The list of arguments to the mixin being included
    /// </summary>
    public readonly List<CallArgument> Arguments;

    /// <summary>
    /// The actions to be slotted into the mixin
    /// </summary>
    public readonly List<Node> SlotActions;

    internal MixinBlockInclude(Coordinate c, string mixinName, List<CallArgument> arguments, List<Node> slotActions) : base(c)
    {
        MixinName = mixinName;
        Arguments = arguments;
        SlotActions = slotActions;
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
                var subEnv = new Environment(environment.GlobalEnvironment, environment)
                {
                    SlotActions = SlotActions
                };
                mixin.Include(subEnv,Arguments.Select(x => x.Compute(environment)).ToList(),selectable,modifiable);
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