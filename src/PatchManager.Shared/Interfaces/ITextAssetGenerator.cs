namespace PatchManager.Shared.Interfaces;

public interface ITextAssetGenerator
{
    /// <summary>
    /// The priority of this patcher compared to other patchers, so it can order the way they get executed
    /// </summary>
    public ulong Priority { get; }
    
    /// <summary>
    /// Creates a new text asset
    /// </summary>
    /// <param name="label">The label of the created asset</param>
    /// <param name="name">The name of the created asset</param>
    /// <returns>The contents of the created asset</returns>
    public string Create(out string label, out string name);
}