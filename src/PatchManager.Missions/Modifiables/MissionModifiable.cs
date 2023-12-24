using PatchManager.Missions.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.Missions.Modifiables;

public class MissionModifiable : JTokenModifiable
{
    private MissionSelectable _missionSelectable;

    public MissionModifiable(MissionSelectable selectable) : base(selectable.MissionObject, selectable.SetModified)
    {
        _missionSelectable = selectable;
    }

    public override void Set(DataValue dataValue)
    {
        if (dataValue.IsDeletion)
        {
            _missionSelectable.SetDeleted();
            return;
        }
        base.Set(dataValue);
    }
}