using JetBrains.Annotations;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Execution;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Builtins;

[SassyLibrary("builtin","functional"),PublicAPI]
public class FunctionalBuiltins
{


    /// <summary>
    /// Gets a function from the global environment
    /// </summary>
    /// <param name="globalEnvironment">The global environment that is automatically filled in</param>
    /// <param name="name">The name of the function to get</param>
    /// <returns>A closure representing that function</returns>
    [SassyMethod("get-function")]
    public static PatchFunction GetFunction(GlobalEnvironment globalEnvironment, string name)
    {
        return globalEnvironment.AllFunctions[name];
    }

    /// <summary>
    /// Invoke a closure w/ the given arguments
    /// </summary>
    /// <param name="env">The environment to run the closure in</param>
    /// <param name="closure"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    [SassyMethod("closure.invoke")]
    public static DataValue Invoke(Environment env, PatchFunction closure, List<PatchArgument> arguments)
    {
        return closure.Execute(env, arguments);
    }


    /// <summary>
    /// Binds arguments to the left side of a closure
    /// </summary>
    /// <param name="closure">The closure to bind the arguments to</param>
    /// <param name="arguments">The arguments to be bound</param>
    /// <returns>A method that is the closure w/ the arguments having been bound</returns>
    [SassyMethod("closure.bind")]
    public static PatchFunction Bind(PatchFunction closure, List<PatchArgument> arguments)
    {
        if (closure is BoundPatchFunction boundPatchFunction)
        {
            var leftBindings = new List<PatchArgument>(boundPatchFunction.LeftBindings);
            leftBindings.AddRange(arguments);
            return new BoundPatchFunction(boundPatchFunction.InternalFunction, leftBindings, boundPatchFunction.RightBindings);
        }
        return new BoundPatchFunction(closure, arguments, new());
    }

    /// <summary>
    /// Binds arguments to the right side of a closure
    /// </summary>
    /// <param name="closure">The closure to bind the arguments to</param>
    /// <param name="arguments">The arguments to be bound</param>
    /// <returns>A method that is the closure w/ the arguments having been bound</returns>
    [SassyMethod("closure.right-bind")]
    public static PatchFunction RightBind(PatchFunction closure, List<PatchArgument> arguments)
    {
        if (closure is BoundPatchFunction boundPatchFunction)
        {
            // Make sure they get bound before the already right bound functions
            // $x:right-bind(1):right-bind(2) should be equivalent to $x:right-bind(2,1)
            var rightBindings = new List<PatchArgument>(arguments);
            rightBindings.AddRange(boundPatchFunction.RightBindings);
            return new BoundPatchFunction(boundPatchFunction.InternalFunction, boundPatchFunction.LeftBindings,rightBindings);
        }

        return new BoundPatchFunction(closure, new(), arguments);
    }
    
}