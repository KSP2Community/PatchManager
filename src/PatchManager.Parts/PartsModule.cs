using JetBrains.Annotations;
using PatchManager.Shared.Modules;

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
    }
}