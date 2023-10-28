using SpaceWarp.API.Configuration;
using UnityEngine.UIElements;

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

    /// <inheritdoc />
    public virtual VisualElement GetDetails() => null;


    /// <inheritdoc />
    public virtual void BindConfiguration(IConfigFile modConfiguration)
    {
    }
}