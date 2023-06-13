using PatchManager.SassyPatching.Nodes;

namespace PatchManager.SassyPatching.Execution;

/// <summary>
/// A patch function that already has an argument bound to it
/// </summary>
public class BoundPatchFunction : PatchFunction
{
    internal readonly PatchFunction InternalFunction;
    internal readonly List<PatchArgument> LeftBindings;
    internal readonly List<PatchArgument> RightBindings;

    internal BoundPatchFunction(PatchFunction internalFunction, List<PatchArgument> leftBindings, List<PatchArgument> rightBindings)
    {
        InternalFunction = internalFunction;
        LeftBindings = leftBindings;
        RightBindings = rightBindings;
    }

    /// <inheritdoc />
    public override DataValue Execute(Environment env, List<PatchArgument> arguments)
    {
        var newArgs = new List<PatchArgument>(LeftBindings);
        newArgs.AddRange(arguments);
        newArgs.AddRange(RightBindings);
        return InternalFunction.Execute(env, newArgs);
    }
}