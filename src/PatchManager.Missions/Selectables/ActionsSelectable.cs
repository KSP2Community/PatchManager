using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Missions.Selectables;

/// <summary>
/// Selectable for the actions array of a mission.
/// </summary>
public sealed class ActionsSelectable : BaseSelectable
{
    /// <summary>
    /// The mission selectable that this actions selectable belongs to.
    /// </summary>
    public MissionSelectable Selectable;

    /// <summary>
    /// The actions array.
    /// </summary>
    public JArray Actions;

    private static string TrimTypeName(string typeName)
    {
        var comma = typeName.IndexOf(',');
        if (comma != -1)
        {
            typeName = typeName[..comma];
        }

        var period = typeName.LastIndexOf('.');
        if (period != -1)
        {
            typeName = typeName[(period + 1)..];
        }

        return typeName;
    }

    /// <summary>
    /// Creates a new actions selectable.
    /// </summary>
    /// <param name="selectable">Mission selectable that this actions selectable belongs to.</param>
    /// <param name="actions">Actions array.</param>
    public ActionsSelectable(MissionSelectable selectable, JArray actions)
    {
        Selectable = selectable;
        Actions = actions;
        Children = new List<ISelectable>();
        Classes = new List<string>();
        foreach (var action in actions)
        {
            var obj = (JObject)action;
            var type = obj["$type"]!.Value<string>()!;
            var trimmedType = TrimTypeName(type);
            Classes.Add(trimmedType);
            Children.Add(new JTokenSelectable(
                Selectable.SetModified,
                obj,
                token => TrimTypeName(((JObject)token)!.Value<string>()!),
                trimmedType
            ));
        }
    }

    /// <inheritdoc />
    public override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name => "actions";

    /// <inheritdoc />
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        foreach (var action in Actions)
        {
            var obj = (JObject)action;
            var type = obj["$type"]!.Value<string>()!;
            var trimmedType = TrimTypeName(type);
            if (trimmedType != @class) continue;
            classValue = DataValue.FromJToken(action);
            return true;
        }

        classValue = DataValue.Null;
        return false;
    }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) =>
        other is ActionsSelectable actionsSelectable && actionsSelectable.Actions == Actions;

    /// <inheritdoc />
    public override IModifiable OpenModification() => new JTokenModifiable(Actions, Selectable.SetModified);

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        var actualType = MissionsTypes.Actions[elementType];
        var elementObject = new JObject()
        {
            ["$type"] = actualType.AssemblyQualifiedName
        };
        foreach (var (key, value) in JObject.FromObject(Activator.CreateInstance(actualType)))
        {
            elementObject[key] = value;
        }

        var selectable = new JTokenSelectable(Selectable.SetModified, elementObject,
            token => TrimTypeName(((JObject)token)!.Value<string>()!), elementType);
        Children.Add(selectable);
        Classes.Add(elementType);
        Actions.Add(elementObject);
        return selectable;
    }

    /// <inheritdoc />
    public override string Serialize() => Actions.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(Actions);

    /// <inheritdoc />
    public override string ElementType => "actions";
}