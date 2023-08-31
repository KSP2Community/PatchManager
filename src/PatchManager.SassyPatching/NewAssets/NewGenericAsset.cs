using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.NewAssets;

/// <summary>
/// Represents a newly made generic asset
/// </summary>
public class NewGenericAsset : INewAsset
{
    /// <summary>
    /// Create a descriptor for a new asset
    /// </summary>
    /// <param name="label">The label of the asset</param>
    /// <param name="name">The name of the asset</param>
    /// <param name="selectable">the selectable of the asset</param>
    public NewGenericAsset(string label, string name, ISelectable selectable)
    {
        Label = label;
        Name = name;
        Selectable = selectable;
    }

    /// <inheritdoc />
    public string Label { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Text => Selectable.Serialize();

    /// <inheritdoc />
    public ISelectable Selectable { get; }
}