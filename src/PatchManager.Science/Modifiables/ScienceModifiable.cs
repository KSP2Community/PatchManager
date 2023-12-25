using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Modifiables;

/// <summary>
/// Modifiable for <see cref="ScienceSelectable"/>. This is used to modify the science object.
/// </summary>
public class ScienceModifiable : JTokenModifiable
{
    private ScienceSelectable _scienceSelectable;

    /// <summary>
    /// Creates a new <see cref="ScienceModifiable"/> for the given <see cref="ScienceSelectable"/>.
    /// </summary>
    /// <param name="selectable">The selectable to modify.</param>
    public ScienceModifiable(ScienceSelectable selectable) : base(selectable.ScienceObject, selectable.SetModified)
    {
        _scienceSelectable = selectable;
    }

    /// <inheritdoc/>
    public override void Set(DataValue dataValue)
    {
        if (dataValue.IsDeletion)
        {
            _scienceSelectable.SetDeleted();
            return;
        }
        base.Set(dataValue);
    }
}