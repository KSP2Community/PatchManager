using ReduxLib.Configuration;
using UnityEngine.UIElements;

namespace PatchManager.Shared.Modules
{
    /// <summary>
    /// Base class for PatchManager modules.
    /// </summary>
    /// <remarks>
    /// Provides empty virtual implementations of every <see cref="IModule" /> member, so derived modules only
    /// override the lifecycle hooks they actually need. Modules that require additional behavior across the
    /// whole interface should implement <see cref="IModule" /> directly instead.
    /// </remarks>
    public class BaseModule : IModule
    {
        /// <inheritdoc />
        public virtual void Init()
        {
        }

        /// <inheritdoc />
        public virtual void RegisterPlaySessionActions()
        {
        }

        /// <inheritdoc />
        public virtual void Load()
        {
        }

        /// <inheritdoc />
        public virtual void PreLoad()
        {

        }

        /// <inheritdoc />
        public virtual VisualElement GetDetails() => null;


        /// <inheritdoc />
        public virtual void BindConfiguration(IConfigFile modConfiguration)
        {
        }
    }
}
