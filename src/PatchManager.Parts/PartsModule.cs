using PatchManager.Shared.Modules;

namespace PatchManager.Parts;

public class PartsModule : BaseModule
{
    public override void Preload()
    {
        PartsUtilities.GrabModuleDataAdapters();
    }
}