using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Modifiables;

/// <summary>
/// Modifiable for <see cref="RegionsSelectable"/>. This is used to modify the discoverables object.
/// </summary>
public class RegionsModifiable : JTokenModifiable
{
    private RegionsSelectable _regionsSelectable;

    /// <summary>
    /// Creates a new <see cref="RegionsModifiable"/> for the given <see cref="RegionsSelectable"/>.
    /// </summary>
    /// <param name="selectable">The selectable to modify.</param>
    public RegionsModifiable(RegionsSelectable selectable) : base(selectable.RegionsObject, selectable.SetModified)
    {
        _regionsSelectable = selectable;
    }

    /// <inheritdoc/>
    public override void Set(DataValue dataValue)
    {
        if (dataValue.IsDeletion)
        {
            _regionsSelectable.SetDeleted();
            return;
        }
        base.Set(dataValue);
    }
}