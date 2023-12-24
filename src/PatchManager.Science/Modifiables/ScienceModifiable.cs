using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Modifiables;

public class ScienceModifiable : JTokenModifiable
{
    private ScienceSelectable _scienceSelectable;

    public ScienceModifiable(ScienceSelectable selectable) : base(selectable.ScienceObject, selectable.SetModified)
    {
        _scienceSelectable = selectable;
    }

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