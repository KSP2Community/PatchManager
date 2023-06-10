using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.SassyPatching.Selectables;

/// <summary>
/// Represents a selectable that operates over <see cref="JToken"/> objects
/// </summary>
public sealed class JTokenSelectable : BaseSelectable
{
    private readonly Action _markDirty;
    private readonly JToken _token;

    /// <summary>
    /// Create a new JToken Selectable
    /// </summary>
    /// <param name="markDirty">How this should notify that it has been modified</param>
    /// <param name="token">The token this operates over</param>
    /// <param name="name">The name/element this is coming from</param>
    public JTokenSelectable(Action markDirty,JToken token, string name)
    {
        _markDirty = markDirty;
        _token = token;
        ElementType = name;
        Classes = new();
        Children = new();
        foreach (var subToken in token)
        {
            if (subToken is not JProperty property) continue;
            Classes.Add(property.Name);
            Children.Add(new JTokenSelectable(markDirty,property.Value,property.Name));
        }
    }

    public override List<ISelectable> Children { get; }
    public override string Name { get; }
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override string ElementType { get; }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) =>
        other is JTokenSelectable jTokenSelectable && jTokenSelectable._token == _token;

    /// <inheritdoc />
    public override IModifiable OpenModification()
    {
        _markDirty();
        return new JTokenModifiable(_token);
    }

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        _token[elementType] = new JObject();
        return new JTokenSelectable(_markDirty, _token[elementType],elementType);
    }

    /// <inheritdoc />
    public override string Serialize() => _token.ToString();
}