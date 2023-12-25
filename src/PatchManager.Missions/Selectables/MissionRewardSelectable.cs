using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Missions.Selectables;

/// <summary>
/// A selectable for a mission reward.
/// </summary>
public sealed class MissionRewardSelectable : BaseSelectable
{
    /// <summary>
    /// The mission selectable this reward belongs to.
    /// </summary>
    public MissionSelectable MissionSelectable;

    /// <summary>
    /// The mission reward.
    /// </summary>
    public JObject MissionReward;

    /// <summary>
    /// The mission reward definitions.
    /// </summary>
    private JArray _missionRewardDefinitions;

    /// <summary>
    /// Creates a new mission reward selectable.
    /// </summary>
    /// <param name="missionSelectable">Mission selectable this reward belongs to.</param>
    /// <param name="missionReward">Mission reward.</param>
    public MissionRewardSelectable(MissionSelectable missionSelectable, JObject missionReward)
    {
        MissionSelectable = missionSelectable;
        MissionReward = missionReward;
        _missionRewardDefinitions = (JArray)missionReward["MissionRewardDefinitions"]!;
        Classes = new List<string>();
        Children = new List<ISelectable>();
        foreach (var child in _missionRewardDefinitions)
        {
            var obj = (JObject)child;
            var name = obj["MissionRewardType"]!.Value<string>()!;
            Classes.Add(name);
            Children.Add(new JTokenSelectable(MissionSelectable.SetModified, child,
                token => ((JObject)token)["MissionRewardType"]!.Value<string>(), name));
        }
    }

    /// <inheritdoc/>
    public override List<ISelectable> Children { get; }

    /// <inheritdoc/>
    public override string Name => "MissionReward";

    /// <inheritdoc/>
    public override List<string> Classes { get; }

    /// <inheritdoc/>
    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        foreach (var child in _missionRewardDefinitions)
        {
            var obj = (JObject)child;
            var name = obj["MissionRewardType"]!.Value<string>()!;
            if (name != @class)
            {
                continue;
            }

            classValue = DataValue.FromJToken(obj);
            return true;
        }

        classValue = DataValue.Null;
        return false;
    }

    /// <inheritdoc/>
    public override bool IsSameAs(ISelectable other) => other is MissionRewardSelectable missionRewardSelectable &&
                                                        missionRewardSelectable.MissionReward == MissionReward;

    /// <inheritdoc/>
    public override IModifiable OpenModification() =>
        new JTokenModifiable(MissionReward, MissionSelectable.SetModified);

    /// <inheritdoc/>
    public override ISelectable AddElement(string elementType)
    {
        var obj = new JObject
        {
            ["MissionRewardType"] = elementType
        };
        Classes.Add(elementType);
        var selectable = new JTokenSelectable(MissionSelectable.SetModified, obj,
            token => ((JObject)token)["MissionRewardType"]!.Value<string>(), elementType);
        Children.Add(selectable);
        _missionRewardDefinitions.Add(obj);
        return selectable;
    }

    /// <inheritdoc/>
    public override string Serialize() => MissionReward.ToString();

    /// <inheritdoc/>
    public override DataValue GetValue() => DataValue.From(_missionRewardDefinitions);

    /// <inheritdoc/>
    public override string ElementType => "MissionReward";
}