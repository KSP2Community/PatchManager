using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Modifiables;

public class ExperimentModifiable : JTokenModifiable
{
    private ExperimentSelectable _experimentSelectable;

    public ExperimentModifiable(ExperimentSelectable selectable) : base(selectable.DataObject, selectable.SetModified)
    {
        _experimentSelectable = selectable;
    }

    public override void Set(DataValue dataValue)
    {
        if (dataValue.IsDeletion)
        {
            _experimentSelectable.SetDeleted();
        }
        base.Set(dataValue);
    }
}