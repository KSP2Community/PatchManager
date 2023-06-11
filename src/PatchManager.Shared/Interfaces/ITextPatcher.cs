namespace PatchManager.Shared.Interfaces;

/// <summary>
/// This interfaces describes a patcher that can patch a simple text asset
/// </summary>
public interface ITextPatcher
{
    /// <summary>
    /// The priority of this patcher compared to other patchers, so it can order the way they get executed
    /// </summary>
    public ulong Priority { get; }

    /// <summary>
    /// Execute this patcher on a blob of text
    /// </summary>
    /// <param name="patchType">The type of patch, "part" for parts patches, etc...</param>
    /// <param name="patchData">The string representation of the file being patched, if it gets set to an empty string, then we should just delete whatever is being patched</param>
    /// <returns>True if the patchData was modified</returns>
    public bool TryPatch(string patchType, ref string patchData);
}