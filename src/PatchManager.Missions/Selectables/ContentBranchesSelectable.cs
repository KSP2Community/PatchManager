using KSP.Game.Missions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.Missions.Selectables;

public sealed class ContentBranchesSelectable : BaseSelectable
{
    public MissionSelectable Selectable;
    public JArray ContentBranches;
    public ContentBranchesSelectable(MissionSelectable selectable, JArray contentBranches)
    {
        Selectable = selectable;
        ContentBranches = contentBranches;
        Children = new();
        Classes = new();
        foreach (var child in ContentBranches)
        {
            var obj = (JObject)child;
            var id = obj["ID"]!.Value<string>()!;
            Classes.Add(id);
            Children.Add(new ContentBranchSelectable(selectable,obj));
        }
    }


    public override List<ISelectable> Children { get; }
    public override string Name => "ContentBranches";
    public override List<string> Classes { get; }

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

    public override bool IsSameAs(ISelectable other) => other is ContentBranchesSelectable contentBranchesSelectable &&
                                                        contentBranchesSelectable.ContentBranches == ContentBranches;

    public override IModifiable OpenModification() => new JTokenModifiable(ContentBranches, Selectable.SetModified);

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

    public override string Serialize() => ContentBranches.ToString();

    public override DataValue GetValue() => DataValue.FromJToken(ContentBranches);

    public override string ElementType => "ContentBranches";
}