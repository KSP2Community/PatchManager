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
    public override void Preload()
    {
        PartsUtilities.GrabModuleDataAdapters();
        SaveLoad.AddFlowActionToCampaignLoadAfter<UpdateSavedVesselPartDefinitions>("Parsing parts text assets");
    }
}