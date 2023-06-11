namespace PatchManager.SassyPatching.Execution;

/// <summary>
/// This is the base class for a patcher function be it dll or file based
/// </summary>
public abstract class PatchFunction
{
    /// <summary>
    /// Execute this function
    /// </summary>
    /// <param name="env">The environment this function is being executed in (used mostly for DLL based functions)</param>
    /// <param name="arguments">The list of arguments for the function, the name can be null</param>
    /// <returns></returns>
    public abstract DataValue Execute(Environment env, List<PatchArgument> arguments);
}