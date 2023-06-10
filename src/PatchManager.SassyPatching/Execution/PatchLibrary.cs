namespace PatchManager.SassyPatching.Execution;

/// <summary>
/// This is the base class of a patcher library, either for file or dll libraries, the register into is what is used by the engine to register everything
/// </summary>
public abstract class PatchLibrary
{
    /// <summary>
    /// Register this library into an environment
    /// </summary>
    /// <param name="environment">The environment to register it into</param>
    public abstract void RegisterInto(Environment environment);
}