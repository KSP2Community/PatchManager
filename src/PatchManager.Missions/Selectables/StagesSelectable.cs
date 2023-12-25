using KSP.Game.Missions.Definitions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.Missions.Selectables;

/// <summary>
/// Selectable for the stages of a mission.
/// </summary>
public sealed class StagesSelectable : BaseSelectable
{
    /// <summary>
    /// The mission selectable this stage selectable belongs to.
    /// </summary>
    public MissionSelectable MissionSelectable;

    /// <summary>
    /// The stages of the mission.
    /// </summary>
    public JArray Stages;

    /// <summary>
    /// Creates a new stages selectable.
    /// </summary>
    /// <param name="selectable">Mission selectable this stage selectable belongs to.</param>
    /// <param name="stages">The stages of the mission.</param>
    public StagesSelectable(MissionSelectable selectable, JArray stages)
    {
        MissionSelectable = selectable;
        Stages = stages;
        Children = new List<ISelectable>();
        Classes = new List<string>();
        foreach (var stage in stages)
        {
            var obj = (JObject)stage;
            var id = obj["StageID"]!.Value<long>();
            var idString = $"_{id}";
            Classes.Add(idString);
            Children.Add(new StageSelectable(selectable, obj));
        }
    }

    /// <inheritdoc/>
    public override List<ISelectable> Children { get; }

    /// <inheritdoc/>
    public override string Name => "missionStages";

    /// <inheritdoc/>
    public override List<string> Classes { get; }

    /// <inheritdoc/>
    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        var num = long.Parse(@class[1..]);
        foreach (var stage in Stages)
        {
            var obj = (JObject)stage;
            var id = obj["ID"]!.Value<long>();
            if (id != num) continue;
            classValue = DataValue.FromJToken(obj);
            return true;
        }
        classValue = DataValue.Null;
        return false;
    }

    /// <inheritdoc/>
    public override bool IsSameAs(ISelectable other) =>
        other is StagesSelectable stagesSelectable && stagesSelectable.Stages == Stages;

    /// <inheritdoc/>
    public override IModifiable OpenModification() => new JTokenModifiable(Stages, MissionSelectable.SetModified);

    /// <inheritdoc/>
    public override ISelectable AddElement(string elementType)
    {
        var num = long.Parse(elementType[1..]);
        var obj = new MissionStage
        {
            StageID = (int)num
        };
        var stage = JObject.FromObject(obj);
        Classes.Add(elementType);
        var selectable = new StageSelectable(MissionSelectable, stage);
        Children.Add(selectable);
        Stages.Add(stage);
        return selectable;
    }

    /// <inheritdoc/>
    public override string Serialize() => Stages.ToString();

    /// <inheritdoc/>
    public override DataValue GetValue() => DataValue.FromJToken(Stages);

    /// <inheritdoc/>
    public override string ElementType => "missionStages";
}