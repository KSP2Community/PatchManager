using KSP.Game.Missions.Definitions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.Missions.Selectables;

public sealed class StagesSelectable : BaseSelectable
{
    public MissionSelectable MissionSelectable;
    public JArray Stages;
    public StagesSelectable(MissionSelectable selectable, JArray stages)
    {
        MissionSelectable = selectable;
        Stages = stages;
        Children = new();
        Classes = new();
        foreach (var stage in stages)
        {
            var obj = (JObject)stage;
            var id = obj["StageID"]!.Value<long>();
            var idString = $"_{id}";
            Classes.Add(idString);
            Children.Add(new StageSelectable(selectable, obj));
        }
    }
    
    public override List<ISelectable> Children { get; }
    public override string Name => "missionStages";
    public override List<string> Classes { get; }

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

    public override bool IsSameAs(ISelectable other) =>
        other is StagesSelectable stagesSelectable && stagesSelectable.Stages == Stages;

    public override IModifiable OpenModification() => new JTokenModifiable(Stages, MissionSelectable.SetModified);

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

    public override string Serialize() => Stages.ToString();

    public override DataValue GetValue() => DataValue.FromJToken(Stages);

    public override string ElementType => "missionStages";
}