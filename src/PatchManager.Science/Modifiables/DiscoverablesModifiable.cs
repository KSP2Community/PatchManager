using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Modifiables;
/// <summary>
/// Modifiable for <see cref="DiscoverablesSelectable"/>. This is used to modify the discoverables object.
/// </summary>
public class DiscoverablesModifiable : JTokenModifiable
{
    private DiscoverablesSelectable _discoverablesSelectable;

    /// <summary>
    /// Creates a new <see cref="DiscoverablesModifiable"/> for the given <see cref="DiscoverablesSelectable"/>.
    /// </summary>
    /// <param name="selectable">The selectable to modify.</param>
    public DiscoverablesModifiable(DiscoverablesSelectable selectable) : base(selectable.DiscoverablesObject, selectable.SetModified)
    {
        _discoverablesSelectable = selectable;
    }

    /// <inheritdoc/>
    public override void Set(DataValue dataValue)
    {
        if (dataValue.IsDeletion)
        {
            _discoverablesSelectable.SetDeleted();
            return;
        }
        base.Set(dataValue);
    }
}