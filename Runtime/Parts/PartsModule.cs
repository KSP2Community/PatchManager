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
        /// Snapshots the available module-data adapters and registers the saved-vessel part-definition update flow action.
        /// </summary>
        public override void Init()
        {
            PartsUtilities.GrabModuleDataAdapters();
            SaveLoad.AddFlowActionToCampaignLoadAfter<UpdateSavedVesselPartDefinitions>("Parsing parts text assets");
        }
    }
}
