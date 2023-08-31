namespace PatchManager.SassyPatching.Interfaces;

/// <summary>
/// Represents a new asset being created
/// </summary>
public interface INewAsset
{
    public string Label { get; }
    public string Name { get; }
    public string Text { get; }

    public ISelectable Selectable { get; }
}