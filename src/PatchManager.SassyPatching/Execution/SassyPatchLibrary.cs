using PatchManager.SassyPatching.Nodes;

namespace PatchManager.SassyPatching.Execution;

internal class SassyPatchLibrary : PatchLibrary
{
    public SassyPatch Patch;

    public SassyPatchLibrary(SassyPatch patch)
    {
        Patch = patch;
    }

    public override void RegisterInto(Environment environment)
    {
        Patch.ExecuteIn(environment);
    }
}