using Newtonsoft.Json.Linq;
using PatchManager.Missions.Modifiables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Missions.Selectables;

/// <summary>
/// A selectable for the main body of a mission
/// </summary>
public sealed class MissionSelectable : BaseSelectable
{
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private bool _modified;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    private bool _deleted;

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

    /// <summary>
    /// Creates a new mission selectable from a JObject
    /// </summary>
    /// <param name="missionObject">The JObject to create the selectable from</param>
    public MissionSelectable(JObject missionObject)
    {
        MissionObject = missionObject;
        Children = new List<ISelectable>();
        Classes = new List<string>();
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

    /// <summary>
    /// The JObject that this selectable represents
    /// </summary>
    public JObject MissionObject;

    /// <inheritdoc/>
    public override List<ISelectable> Children { get; }

    /// <inheritdoc/>
    public override string Name => MissionObject["ID"]!.Value<string>()!;

    /// <inheritdoc/>
    public override List<string> Classes { get; }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override bool IsSameAs(ISelectable other) =>
        other is MissionSelectable selectable && selectable.MissionObject == MissionObject;

    /// <inheritdoc/>
    public override IModifiable OpenModification() => new MissionModifiable(this);

    /// <inheritdoc/>
    public override ISelectable AddElement(string elementType) => throw new Exception(
        "You cannot add elements to the main body of the mission, try using ContentBranches, or missionStages for that");

    /// <inheritdoc/>
    public override string Serialize() => _deleted ? "" : MissionObject.ToString();

    /// <inheritdoc/>
    public override DataValue GetValue() => DataValue.FromJToken(MissionObject);

    /// <inheritdoc/>
    public override string ElementType => "missions";
}