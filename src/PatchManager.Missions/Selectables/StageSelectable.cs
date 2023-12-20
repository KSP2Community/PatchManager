using KSP.Game.Missions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Missions.Selectables;

public sealed class StageSelectable : BaseSelectable
{
    public MissionSelectable MissionSelectable;

    public JObject StageObject;

    private int ConditionIndex = 0;
    
    public StageSelectable(MissionSelectable missionSelectable, JObject stageObject)
    {
        MissionSelectable = missionSelectable;
        StageObject = stageObject;
        Children = new();
        Classes = new();
        foreach (var child in stageObject)
        {
            Classes.Add(child.Key);
            if (child.Key != "scriptableCondition" && child.Key != "actions" && child.Key != "MissionReward")
            {
                Children.Add(new JTokenSelectable(MissionSelectable.SetModified, child.Value, child.Key, child.Key));
            }
        }

        ConditionIndex = Children.Count;
        if (((JObject)stageObject["scriptableCondition"]!)["ConditionType"]!.Value<string>() == "ConditionSet")
        {
            Children.Add(new ConditionSetSelectable(missionSelectable,(JObject)stageObject["scriptableCondition"]!));
        }
        else
        {
            Children.Add(new JTokenSelectable(MissionSelectable.SetModified, stageObject["scriptableCondition"],
                "scriptableCondition", "scriptableCondition"));
        }
        Children.Add(new MissionRewardSelectable(MissionSelectable,(JObject)stageObject["MissionReward"]!));
        Children.Add(new ActionsSelectable(MissionSelectable,(JArray)stageObject["actions"]!));
    }

    public override List<ISelectable> Children { get; }
    public override string Name => $"_{StageObject["StageID"]!.Value<long>()}";
    public override List<string> Classes { get; }
    public override bool MatchesClass(string @class, out DataValue classValue) => throw new NotImplementedException();

    public override bool IsSameAs(ISelectable other) => throw new NotImplementedException();

    public override IModifiable OpenModification() => throw new NotImplementedException();

    public override ISelectable AddElement(string elementType)
    {
        var conditionType = MissionsTypes.Conditions[elementType];
        var conditionObject = JObject.FromObject(Activator.CreateInstance(conditionType));
        conditionObject["$type"] = conditionType.AssemblyQualifiedName;
        if (conditionType == typeof(ConditionSet))
        {
            return Children[ConditionIndex] = new ConditionSetSelectable(MissionSelectable, conditionObject);
        }

        StageObject["scriptableCondition"] = conditionObject;
        return Children[ConditionIndex] = new JTokenSelectable(MissionSelectable.SetModified, conditionObject,
            "scriptableCondition", "scriptableCondition");
    }

    public override string Serialize() => StageObject.ToString();

    public override DataValue GetValue() => DataValue.From(StageObject);

    public override string ElementType => $"_{StageObject["StageID"]!.Value<long>()}";
}