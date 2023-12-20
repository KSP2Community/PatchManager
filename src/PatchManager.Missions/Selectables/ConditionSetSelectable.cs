using KSP.Game.Missions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Missions.Selectables;

public sealed class ConditionSetSelectable : BaseSelectable
{
    public MissionSelectable MissionSelectable;
    public JObject ConditionSet;
    private JArray _children;
    public ConditionSetSelectable(MissionSelectable missionSelectable, JObject conditionSet)
    {
        MissionSelectable = missionSelectable;
        ConditionSet = conditionSet;
        _children = (JArray)conditionSet["Children"]!;
        Children = new();
        Classes = new();
        Classes.Add("ConditionType");
        Classes.Add("ConditionMode");
        // We aren't going to add the condition type as a child, it will still be editable tho
        foreach (var child in _children)
        {
            var condition = (JObject)child;
            var type = condition["ConditionType"]!.Value<string>()!;
            Classes.Add(type);
            if (type == "ConditionSet")
            {
                Children.Add(new ConditionSetSelectable(missionSelectable,condition));
            }
            else
            {
                Children.Add(new JTokenSelectable(MissionSelectable.SetModified, condition,
                    token => ((JObject)token)["ConditionType"]!.Value<string>()!, type));
            }
        }
    }

    public override List<ISelectable> Children { get; }
    public override string Name => "ConditionSet";
    public override List<string> Classes { get; }

    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        foreach (var child in _children)
        {
            var condition = (JObject)child;
            var type = condition["ConditionType"]!.Value<string>()!;
            if (type != @class)
                continue;
            classValue = DataValue.FromJToken(child);
        }
        classValue = DataValue.Null;
        return false;
    }

    public override bool IsSameAs(ISelectable other) => other is ConditionSetSelectable conditionSetSelectable &&
                                                        conditionSetSelectable.ConditionSet == ConditionSet;

    public override IModifiable OpenModification() => new JTokenModifiable(ConditionSet, MissionSelectable.SetModified);

    public override ISelectable AddElement(string elementType)
    {
        var conditionType = MissionsTypes.Conditions[elementType];
        var conditionObject = JObject.FromObject(Activator.CreateInstance(conditionType));
        conditionObject["$type"] = conditionType.AssemblyQualifiedName;
        _children.Add(conditionObject);
        if (conditionType == typeof(ConditionSet))
        {
            var selectable  = new ConditionSetSelectable(MissionSelectable, conditionObject);
            Children.Add(selectable);
            return selectable;
        }
        else
        {
            var selectable = new JTokenSelectable(MissionSelectable.SetModified, conditionObject,
                "scriptableCondition", "scriptableCondition");
            Children.Add(selectable);
            return selectable;
        }
    }

    public override string Serialize() => ConditionSet.ToString();

    public override DataValue GetValue() => DataValue.FromJToken(ConditionSet);

    public override string ElementType => "ConditionSet";
}