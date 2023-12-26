using KSP.Game.Missions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.Missions.Selectables;

/// <summary>
/// Selectable for the ContentBranches of a Mission
/// </summary>
public sealed class ContentBranchesSelectable : BaseSelectable
{
    /// <summary>
    /// The MissionSelectable this ContentBranchesSelectable belongs to
    /// </summary>
    public MissionSelectable Selectable;

    /// <summary>
    /// The ContentBranches of the Mission
    /// </summary>
    public JArray ContentBranches;

    /// <summary>
    /// Create a new ContentBranchesSelectable
    /// </summary>
    /// <param name="selectable">Mission selectable this ContentBranchesSelectable belongs to</param>
    /// <param name="contentBranches">Content branches of the mission</param>
    public ContentBranchesSelectable(MissionSelectable selectable, JArray contentBranches)
    {
        Selectable = selectable;
        ContentBranches = contentBranches;
        Children = new List<ISelectable>();
        Classes = new List<string>();
        foreach (var child in ContentBranches)
        {
            var obj = (JObject)child;
            var id = obj["ID"]!.Value<string>()!;
            Classes.Add(id);
            Children.Add(new ContentBranchSelectable(selectable,obj));
        }
    }

    /// <inheritdoc/>
    public override List<ISelectable> Children { get; }

    /// <inheritdoc/>
    public override string Name => "ContentBranches";

    /// <inheritdoc/>
    public override List<string> Classes { get; }

    /// <inheritdoc/>
    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        foreach (var child in ContentBranches)
        {
            var obj = (JObject)child;
            var id = obj["ID"]!.Value<string>()!;
            if (id != @class)
                continue;
            classValue = DataValue.FromJToken(obj);
            return true;
        }

        classValue = DataValue.Null;
        return false;
    }

    /// <inheritdoc/>
    public override bool IsSameAs(ISelectable other) => other is ContentBranchesSelectable contentBranchesSelectable &&
                                                        contentBranchesSelectable.ContentBranches == ContentBranches;

    /// <inheritdoc/>
    public override IModifiable OpenModification() => new JTokenModifiable(ContentBranches, Selectable.SetModified);

    /// <inheritdoc/>
    public override ISelectable AddElement(string elementType)
    {
        var branch = new MissionContentBranch
        {
            ID = elementType
        };
        var obj = JObject.FromObject(branch);
        var selectable = new ContentBranchSelectable(Selectable, obj);
        Children.Add(selectable);
        Classes.Add(elementType);
        ContentBranches.Add(obj);
        return selectable;
    }

    /// <inheritdoc/>
    public override string Serialize() => ContentBranches.ToString();

    /// <inheritdoc/>
    public override DataValue GetValue() => DataValue.FromJToken(ContentBranches);

    /// <inheritdoc/>
    public override string ElementType => "ContentBranches";
}