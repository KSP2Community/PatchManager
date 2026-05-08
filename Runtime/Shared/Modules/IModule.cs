using JetBrains.Annotations;
using ReduxLib.Configuration;
using UnityEngine.UIElements;

namespace PatchManager.Shared.Modules
{
    /// <summary>
    /// Interface for PatchManager DLL modules.
    /// </summary>
    /// <remarks>
    /// Each PatchManager module assembly contains exactly one type that implements this interface;
    /// <see cref="ModuleManager" /> discovers and instantiates it via reflection at module-registration time.
    /// Lifecycle methods fire in the order <see cref="Init" /> -> <see cref="PreLoad" /> -> <see cref="Load" />,
    /// each at a different stage of the host mod's bootstrap. Implementations that only need a subset of the
    /// lifecycle should derive from <see cref="BaseModule" /> rather than implementing the interface directly.
    /// </remarks>
    public interface IModule
    {
        /// <summary>
        /// Called during the host mod's Start, before the game itself is loaded.
        /// </summary>
        /// <remarks>
        /// Implementations typically use this to register actions with the FlowManager.
        /// </remarks>
        public void Init();
    
        /// <summary>
        /// Called from the host mod's OnPreInitialized, the first SpaceWarp init stage, before the game is loaded.
        /// </summary>
        /// <remarks>
        /// Implementations typically use this for setup that must run before the game finishes loading,
        /// such as constructing the patch universe or registering state that later init steps depend on.
        /// </remarks>
        public void PreLoad();
    
        /// <summary>
        /// Called from the host mod's OnInitialized, the second SpaceWarp init stage, after the game and mod assets are loaded.
        /// </summary>
        /// <remarks>
        /// Implementations typically use this to register resource locators and perform setup that requires
        /// a fully initialized GameInstance.
        /// </remarks>
        public void Load();

        /// <summary>
        /// Returns the visual element to embed in PatchManager's mod-list details foldout for this module.
        /// </summary>
        /// <remarks>
        /// Invoked once during post-initialization, when PatchManager assembles the per-module diagnostics panel.
        /// Implementations should return <c>null</c> when the module has nothing to display.
        /// </remarks>
        /// <returns>The visual element describing the module, or <c>null</c> if the module has nothing to report.</returns>
        [CanBeNull]
        public VisualElement GetDetails();

        /// <summary>
        /// Called once during host-mod startup to let the module bind its configuration entries.
        /// </summary>
        /// <remarks>
        /// All PatchManager modules share the host mod's single <see cref="IConfigFile" />, so implementations should
        /// namespace their config sections to avoid collisions. Invoked before <see cref="Init" />.
        /// </remarks>
        /// <param name="modConfiguration">The host PatchManager mod's configuration file.</param>
        public void BindConfiguration(IConfigFile modConfiguration);
    }
}