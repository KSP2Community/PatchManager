using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.Missions.Selectables;

public sealed class ContentBranchSelectable : BaseSelectable
{
    public MissionSelectable Selectable;
    public JObject ContentBranch;
    private ActionsSelectable _actionsSelectable;
    public ContentBranchSelectable(MissionSelectable selectable, JObject contentBranch)
    {
        Selectable = selectable;
        ContentBranch = contentBranch;
        Classes = new();
        Classes.Add("actions");
        Children = new();
        _actionsSelectable = new ActionsSelectable(selectable, (JArray)contentBranch["actions"]!);
        Children.Add(_actionsSelectable);
        Classes.AddRange(_actionsSelectable.Classes);
        Children.AddRange(_actionsSelectable.Children);
    }

    public override List<ISelectable> Children { get; }
    public override string Name => ContentBranch["ID"]!.Value<string>()!;
    public override List<string> Classes { get; }

    public override bool MatchesClass(string @class, out DataValue classValue) =>
        _actionsSelectable.MatchesClass(@class, out classValue);

    public override bool IsSameAs(ISelectable other) => other is ContentBranchSelectable contentBranchSelectable &&
                                                        contentBranchSelectable.ContentBranch == ContentBranch;

    public override IModifiable OpenModification() => new JTokenModifiable(ContentBranch, Selectable.SetModified);

    public override ISelectable AddElement(string elementType)
    {
        var result = _actionsSelectable.AddElement(elementType);
        Children.Add(result);
        Classes.Add(elementType);
        return result;
    }

    public override string Serialize() => ContentBranch.ToString();

    public override DataValue GetValue() => DataValue.FromJToken(ContentBranch);

    public override string ElementType => ContentBranch["ID"]!.Value<string>()!;
}