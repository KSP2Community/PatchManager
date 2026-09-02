using JetBrains.Annotations;
using PatchManager.Parts.Patchers;
using PatchManager.Shared.Modules;
using SpaceWarp2.API.Loading;

namespace PatchManager.Parts
{
    /// <summary>
    /// Part patching module.
    /// </summary>
    [UsedImplicitly]
    public class PartsModule : BaseModule
    {
        /// <summary>
        /// Snapshots the available module-data adapters.
        /// </summary>
        public override void Init()
        {
            PartsUtilities.GrabModuleDataAdapters();
        }

        /// <summary>
        /// Registers the saved-vessel part-definition update action for the current play session.
        /// </summary>
        public override void RegisterPlaySessionActions()
        {
            SaveLoad.AddFlowActionToCampaignLoadAfter<UpdateSavedVesselPartDefinitions>("Parsing parts text assets");
        }
    }
}
