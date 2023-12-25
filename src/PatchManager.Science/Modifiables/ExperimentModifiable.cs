using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Modifiables;

/// <summary>
/// Modifiable for <see cref="ExperimentSelectable"/>.
/// </summary>
public class ExperimentModifiable : JTokenModifiable
{
    /// <summary>
    /// The <see cref="ExperimentSelectable"/> this modifiable is for.
    /// </summary>
    private ExperimentSelectable _experimentSelectable;

    /// <summary>
    /// Creates a new <see cref="ExperimentModifiable"/> for the given <see cref="ExperimentSelectable"/>.
    /// </summary>
    /// <param name="selectable">The <see cref="ExperimentSelectable"/> this modifiable is for.</param>
    public ExperimentModifiable(ExperimentSelectable selectable) : base(selectable.DataObject, selectable.SetModified)
    {
        _experimentSelectable = selectable;
    }

    /// <inheritdoc/>
    public override void Set(DataValue dataValue)
    {
        if (dataValue.IsDeletion)
        {
            _experimentSelectable.SetDeleted();
            return;
        }
        base.Set(dataValue);
    }
}