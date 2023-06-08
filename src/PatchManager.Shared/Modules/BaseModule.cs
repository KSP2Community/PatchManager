namespace PatchManager.Shared.Modules;

/// <summary>
/// Base class for PatchManager modules.
/// </summary>
public class BaseModule : IModule
{
    /// <inheritdoc/>
    public virtual void Preload()
    {
    }

    /// <inheritdoc/>
    public virtual void Load()
    {
    }
}