using KSP.Game.Missions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Missions.Selectables;

public sealed class StageSelectable : BaseSelectable
{
    public MissionSelectable MissionSelectable;

    public JObject StageObject;

    private int ConditionIndex = -1;
    
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

        if (stageObject["scriptableCondition"].Type == JTokenType.Object)
        {
            if (((JObject)stageObject["scriptableCondition"]!)["ConditionType"]!.Value<string>() == "ConditionSet")
            {

                ConditionIndex = Children.Count;
                Children.Add(
                    new ConditionSetSelectable(missionSelectable, (JObject)stageObject["scriptableCondition"]!));
            }
            else
            {

                ConditionIndex = Children.Count;
                Children.Add(new JTokenSelectable(MissionSelectable.SetModified, stageObject["scriptableCondition"],
                    "scriptableCondition", "scriptableCondition"));
            }
        }

        if (stageObject.TryGetValue("MissionReward", out var value))
        {
            Children.Add(new MissionRewardSelectable(MissionSelectable, (JObject)value!));
        }

        Children.Add(new ActionsSelectable(MissionSelectable,(JArray)stageObject["actions"]!));
    }

    public override List<ISelectable> Children { get; }
    public override string Name => $"_{StageObject["StageID"]!.Value<long>()}";
    public override List<string> Classes { get; }

    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        if (StageObject.TryGetValue(@class, out var jToken))
        {
            classValue = DataValue.FromJToken(jToken);
            return true;
        }
        classValue = DataValue.Null;
        return false;
    }

    public override bool IsSameAs(ISelectable other) =>
        other is StageSelectable stageSelectable && stageSelectable.StageObject == StageObject;

    public override IModifiable OpenModification() => new JTokenModifiable(StageObject,MissionSelectable.SetModified);

    public override ISelectable AddElement(string elementType)
    {
        var conditionType = MissionsTypes.Conditions[elementType];
        // var conditionObject = JObject.FromObject(Activator.CreateInstance(conditionType));
        var conditionObject = new JObject();
        foreach (var (key, value) in JObject.FromObject(Activator.CreateInstance(conditionType)))
        {
            conditionObject[key] = value;
        }
        StageObject["scriptableCondition"] = conditionObject;
        if (conditionType == typeof(ConditionSet))
        {
            var selectable = new ConditionSetSelectable(MissionSelectable, conditionObject);
            if (ConditionIndex > 0)
            {
                return Children[ConditionIndex] = selectable;
            }

            ConditionIndex = Children.Count;
            Children.Add(selectable);
            return selectable;
        }
        else
        {
            
            var selectable = new JTokenSelectable(MissionSelectable.SetModified, conditionObject,
                "scriptableCondition", "scriptableCondition");
            
            if (ConditionIndex > 0)
            {
                return Children[ConditionIndex] = selectable;
            }

            ConditionIndex = Children.Count;
            Children.Add(selectable);
            return selectable;
        }

    }

    public override string Serialize() => StageObject.ToString();

    public override DataValue GetValue() => DataValue.From(StageObject);

    public override string ElementType => $"_{StageObject["StageID"]!.Value<long>()}";
}