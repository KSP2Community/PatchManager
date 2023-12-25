using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.Missions.Selectables;

/// <summary>
/// A selectable for a content branch.
/// </summary>
public sealed class ContentBranchSelectable : BaseSelectable
{
    /// <summary>
    /// The mission selectable that this content branch belongs to.
    /// </summary>
    public MissionSelectable Selectable;

    /// <summary>
    /// The content branch that this selectable represents.
    /// </summary>
    public JObject ContentBranch;

    /// <summary>
    /// The actions selectable that this content branch contains.
    /// </summary>
    private ActionsSelectable _actionsSelectable;

    /// <summary>
    /// Creates a new content branch selectable.
    /// </summary>
    /// <param name="selectable">Mission selectable that this content branch belongs to.</param>
    /// <param name="contentBranch">Content branch that this selectable represents.</param>
    public ContentBranchSelectable(MissionSelectable selectable, JObject contentBranch)
    {
        Selectable = selectable;
        ContentBranch = contentBranch;
        Classes = new List<string> { "actions" };
        Children = new List<ISelectable>();
        _actionsSelectable = new ActionsSelectable(selectable, (JArray)contentBranch["actions"]!);
        Children.Add(_actionsSelectable);
        Classes.AddRange(_actionsSelectable.Classes);
        Children.AddRange(_actionsSelectable.Children);
    }

    /// <inheritdoc />
    public override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name => ContentBranch["ID"]!.Value<string>()!;

    /// <inheritdoc />
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override bool MatchesClass(string @class, out DataValue classValue) =>
        _actionsSelectable.MatchesClass(@class, out classValue);

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) => other is ContentBranchSelectable contentBranchSelectable &&
                                                        contentBranchSelectable.ContentBranch == ContentBranch;

    /// <inheritdoc />
    public override IModifiable OpenModification() => new JTokenModifiable(ContentBranch, Selectable.SetModified);

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        var result = _actionsSelectable.AddElement(elementType);
        Children.Add(result);
        Classes.Add(elementType);
        return result;
    }

    /// <inheritdoc />
    public override string Serialize() => ContentBranch.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(ContentBranch);

    /// <inheritdoc />
    public override string ElementType => ContentBranch["ID"]!.Value<string>()!;
}