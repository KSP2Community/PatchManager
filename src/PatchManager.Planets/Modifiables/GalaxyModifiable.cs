namespace PatchManager.Planets.Modifiables;

using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.Planets.Selectables;


/// <summary>
/// Modifiable for <see cref="GalaxySelectable"/>. This is used to modify the science object.
/// </summary>
public class GalaxyModifiable : JTokenModifiable
{
    private GalaxySelectable _galaxySelectable;

    /// <summary>
    /// Creates a new <see cref="GalaxyModifiable"/> for the given <see cref="GalaxyModifiable"/>.
    /// </summary>
    /// <param name="selectable">The selectable to modify.</param>
    public GalaxyModifiable(GalaxySelectable selectable) : base(selectable.GalaxyObject, selectable.SetModified) => _galaxySelectable = selectable;

    /// <inheritdoc/>
    public override void Set(DataValue dataValue)
    {
        if (dataValue.IsDeletion)
        {
            _galaxySelectable.SetDeleted();
            return;
        }
        base.Set(dataValue);
    }
}