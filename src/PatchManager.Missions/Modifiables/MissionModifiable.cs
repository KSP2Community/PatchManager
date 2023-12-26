using PatchManager.Missions.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.Missions.Modifiables;

/// <summary>
/// Modifiable for <see cref="MissionSelectable"/>. This is used to modify the mission object.
/// </summary>
public class MissionModifiable : JTokenModifiable
{
    /// <summary>
    /// The <see cref="MissionSelectable"/> that this modifiable is for.
    /// </summary>
    private readonly MissionSelectable _missionSelectable;

    /// <summary>
    /// Creates a new <see cref="MissionModifiable"/> for the given <see cref="MissionSelectable"/>.
    /// </summary>
    /// <param name="selectable">The <see cref="MissionSelectable"/> to create the modifiable for.</param>
    public MissionModifiable(MissionSelectable selectable) : base(selectable.MissionObject, selectable.SetModified)
    {
        _missionSelectable = selectable;
    }

    /// <summary>
    /// Sets the <see cref="MissionSelectable"/> to be deleted if the <see cref="DataValue"/> is a deletion.
    /// </summary>
    /// <param name="dataValue">The <see cref="DataValue"/> to set.</param>
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