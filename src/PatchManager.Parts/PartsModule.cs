using JetBrains.Annotations;
using PatchManager.Parts.Patchers;
using PatchManager.Shared.Modules;
using SpaceWarp.API.Loading;

namespace PatchManager.Parts;

/// <summary>
/// Part patching module.
/// </summary>
[UsedImplicitly]
public class PartsModule : BaseModule
{
    /// <summary>
    /// Initialize the module
    /// </summary>
    public override void Init()
    {
        PartsUtilities.GrabModuleDataAdapters();
        SaveLoad.AddFlowActionToCampaignLoadAfter<UpdateSavedVesselPartDefinitions>("Parsing parts text assets");
    }
}