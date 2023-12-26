using KSP.Game.Missions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Missions.Selectables;

/// <summary>
/// Selectable for the condition set of a mission.
/// </summary>
public sealed class ConditionSetSelectable : BaseSelectable
{
    /// <summary>
    /// The mission selectable that this condition set is a child of
    /// </summary>
    public MissionSelectable MissionSelectable;

    /// <summary>
    /// The condition set that this selectable represents
    /// </summary>
    public JObject ConditionSet;

    /// <summary>
    /// The children of this condition set
    /// </summary>
    private JArray _children;

    /// <summary>
    /// Create a new condition set selectable
    /// </summary>
    /// <param name="missionSelectable">Mission selectable that this condition set is a child of</param>
    /// <param name="conditionSet">The condition set that this selectable represents</param>
    public ConditionSetSelectable(MissionSelectable missionSelectable, JObject conditionSet)
    {
        MissionSelectable = missionSelectable;
        ConditionSet = conditionSet;
        _children = (JArray)conditionSet["Children"]!;
        Children = new List<ISelectable>();
        Classes = new List<string>
        {
            "ConditionType",
            "ConditionMode"
        };
        // We aren't going to add the condition type as a child, it will still be editable tho
        foreach (var child in _children)
        {
            var condition = (JObject)child;
            var type = condition["ConditionType"]!.Value<string>()!;
            Classes.Add(type);
            if (type == "ConditionSet")
            {
                Children.Add(new ConditionSetSelectable(missionSelectable, condition));
            }
            else
            {
                Children.Add(new JTokenSelectable(
                    MissionSelectable.SetModified,
                    condition,
                    token => ((JObject)token)["ConditionType"]!.Value<string>()!,
                    type
                ));
            }
        }
    }

    /// <inheritdoc />
    public override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name => "ConditionSet";

    /// <inheritdoc />
    public override List<string> Classes { get; }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) => other is ConditionSetSelectable conditionSetSelectable &&
                                                        conditionSetSelectable.ConditionSet == ConditionSet;

    /// <inheritdoc />
    public override IModifiable OpenModification() => new JTokenModifiable(ConditionSet, MissionSelectable.SetModified);

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        var conditionType = MissionsTypes.Conditions[elementType];
        var conditionObject = new JObject()
        {
            ["$type"] = conditionType.AssemblyQualifiedName
        };
        foreach (var (key, value) in JObject.FromObject(Activator.CreateInstance(conditionType)))
        {
            conditionObject[key] = value;
        }

        _children.Add(conditionObject);
        if (conditionType == typeof(ConditionSet))
        {
            var selectable = new ConditionSetSelectable(MissionSelectable, conditionObject);
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

    /// <inheritdoc />
    public override string Serialize() => ConditionSet.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(ConditionSet);

    /// <inheritdoc />
    public override string ElementType => "ConditionSet";
}