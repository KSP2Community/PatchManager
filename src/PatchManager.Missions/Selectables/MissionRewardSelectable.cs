using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Missions.Selectables;

public sealed class MissionRewardSelectable : BaseSelectable
{
    public MissionSelectable MissionSelectable;
    public JObject MissionReward;
    private JArray _missionRewardDefinitions;
    public MissionRewardSelectable(MissionSelectable missionSelectable, JObject missionReward)
    {
        MissionSelectable = missionSelectable;
        MissionReward = missionReward;
        _missionRewardDefinitions = (JArray)missionReward["MissionRewardDefinitions"]!;
        Classes = new();
        Children = new();
        foreach (var child in _missionRewardDefinitions)
        {
            var obj = (JObject)child;
            var name = obj["MissionRewardType"]!.Value<string>()!;
            Classes.Add(name);
            Children.Add(new JTokenSelectable(MissionSelectable.SetModified,child, token => ((JObject)token)["MissionRewardType"]!.Value<string>(),name));
        }
    }
    public override List<ISelectable> Children { get; }
    public override string Name => "MissionReward";
    public override List<string> Classes { get; }

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

    public override bool IsSameAs(ISelectable other) => throw new NotImplementedException();

    public override IModifiable OpenModification() => throw new NotImplementedException();

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

    public override string Serialize() => MissionReward.ToString();

    public override DataValue GetValue() => DataValue.From(_missionRewardDefinitions);

    public override string ElementType => "MissionReward";
}