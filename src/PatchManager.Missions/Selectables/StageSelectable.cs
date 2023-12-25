using KSP.Game.Missions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Missions.Selectables;

/// <summary>
/// Selectable for a stage in a mission.
/// </summary>
public sealed class StageSelectable : BaseSelectable
{
    /// <summary>
    /// The mission selectable that this stage belongs to.
    /// </summary>
    public MissionSelectable MissionSelectable;

    /// <summary>
    /// The stage object.
    /// </summary>
    public JObject StageObject;

    private int _conditionIndex = -1;

    /// <summary>
    /// Create a new stage selectable.
    /// </summary>
    /// <param name="missionSelectable">Mission selectable that this stage belongs to.</param>
    /// <param name="stageObject">Stage object.</param>
    public StageSelectable(MissionSelectable missionSelectable, JObject stageObject)
    {
        MissionSelectable = missionSelectable;
        StageObject = stageObject;
        Children = new List<ISelectable>();
        Classes = new List<string>();
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
                _conditionIndex = Children.Count;
                Children.Add(
                    new ConditionSetSelectable(missionSelectable, (JObject)stageObject["scriptableCondition"]!));
            }
            else
            {
                _conditionIndex = Children.Count;
                Children.Add(new JTokenSelectable(MissionSelectable.SetModified, stageObject["scriptableCondition"],
                    "scriptableCondition", "scriptableCondition"));
            }
        }

        if (stageObject.TryGetValue("MissionReward", out var value))
        {
            Children.Add(new MissionRewardSelectable(MissionSelectable, (JObject)value!));
        }

        Children.Add(new ActionsSelectable(MissionSelectable, (JArray)stageObject["actions"]!));
    }

    /// <inheritdoc/>
    public override List<ISelectable> Children { get; }

    /// <inheritdoc/>
    public override string Name => $"_{StageObject["StageID"]!.Value<long>()}";

    /// <inheritdoc/>
    public override List<string> Classes { get; }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override bool IsSameAs(ISelectable other) =>
        other is StageSelectable stageSelectable && stageSelectable.StageObject == StageObject;

    /// <inheritdoc/>
    public override IModifiable OpenModification() => new JTokenModifiable(StageObject, MissionSelectable.SetModified);

    /// <inheritdoc/>
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
            if (_conditionIndex > 0)
            {
                return Children[_conditionIndex] = selectable;
            }

            _conditionIndex = Children.Count;
            Children.Add(selectable);
            return selectable;
        }
        else
        {
            var selectable = new JTokenSelectable(
                MissionSelectable.SetModified,
                conditionObject,
                "scriptableCondition",
                "scriptableCondition"
            );

            if (_conditionIndex > 0)
            {
                return Children[_conditionIndex] = selectable;
            }

            _conditionIndex = Children.Count;
            Children.Add(selectable);
            return selectable;
        }
    }

    /// <inheritdoc/>
    public override string Serialize() => StageObject.ToString();

    /// <inheritdoc/>
    public override DataValue GetValue() => DataValue.From(StageObject);

    /// <inheritdoc/>
    public override string ElementType => $"_{StageObject["StageID"]!.Value<long>()}";
}