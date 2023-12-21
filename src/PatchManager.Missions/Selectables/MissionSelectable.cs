using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using PatchManager.Missions.Modifiables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Missions.Selectables;

public sealed class MissionSelectable : BaseSelectable
{
    private bool _modified = false;
    private bool _deleted = false;


    /// <summary>
    /// Marks this part selectable as having been modified any level down
    /// </summary>
    public void SetModified()
    {
        _modified = true;
    }
    
    /// <summary>
    /// Marks this part as goneso
    /// </summary>
    public void SetDeleted()
    {
        SetModified();
        _deleted = true;
    }

    public MissionSelectable(JObject missionObject)
    {
        MissionObject = missionObject;
        Children = new();
        Classes = new();
        foreach (var child in missionObject)
        {
            Classes.Add(child.Key);
            if (child.Key != "missionStages" && child.Key != "ContentBranches")
            {
                Children.Add(new JTokenSelectable(SetModified, child.Value, child.Key, child.Key));
            }
        }

        var stages = (JArray)MissionObject["missionStages"]!;
        Children.Add(new StagesSelectable(this, stages));
        if (missionObject.ContainsKey("ContentBranches"))
        {
            var branches = (JArray)MissionObject["ContentBranches"]!;
            Children.Add(new ContentBranchesSelectable(this, branches));
        }
    }
    
    public JObject MissionObject;

    public override List<ISelectable> Children { get; }
    public override string Name => MissionObject["ID"]!.Value<string>()!;
    public override List<string> Classes { get; }

    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        if (MissionObject.TryGetValue(@class, out var jToken))
        {
            classValue = DataValue.FromJToken(jToken);
            return true;
        }
        classValue = DataValue.Null;
        return false;
    }

    public override bool IsSameAs(ISelectable other) =>
        other is MissionSelectable selectable && selectable.MissionObject == MissionObject;

    public override IModifiable OpenModification() => new MissionModifiable(this);

    public override ISelectable AddElement(string elementType) => throw new Exception(
        "You cannot add elements to the main body of the mission, try using ContentBranches, or missionStages for that");

    public override string Serialize() => _deleted ? "" : MissionObject.ToString();

    public override DataValue GetValue() => DataValue.FromJToken(MissionObject);

    public override string ElementType => "missions";
}