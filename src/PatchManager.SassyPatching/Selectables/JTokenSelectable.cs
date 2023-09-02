using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.SassyPatching.Selectables;

/// <summary>
/// Represents a selectable that operates over <see cref="JToken"/> objects
/// </summary>
public class JTokenSelectable : BaseSelectable
{
    private readonly Action _markDirty;
    /// <summary>
    /// This is the token being modified
    /// </summary>
    protected readonly JToken Token;

    /// <summary>
    /// Create a new JToken Selectable
    /// </summary>
    /// <param name="markDirty">How this should notify that it has been modified</param>
    /// <param name="token">The token this operates over</param>
    /// <param name="name">The name/element this is coming from</param>
    /// <param name="elementType">The type of element this is, can be different from the name</param>
    public JTokenSelectable(Action markDirty, JToken token, string name, [CanBeNull] string elementType = null)
    {
        _markDirty = markDirty;
        Token = token;
        ElementType = elementType ?? name;
        getName = _ => name;
        Classes = new();
        Children = new();
        foreach (var subToken in token)
        {
            if (subToken is not JProperty property) continue;
            Classes.Add(property.Name);
            Children.Add(new JTokenSelectable(markDirty,property.Value,property.Name));
        }
    }
    
    /// <summary>
    /// Create a new JToken Selectable
    /// </summary>
    /// <param name="markDirty">How this should notify that it has been modified</param>
    /// <param name="token">The token this operates over</param>
    /// <param name="name">The function used to get the name of this token</param>
    /// <param name="elementType">The type of element this is, can be different from the name</param>
    public JTokenSelectable(Action markDirty, JToken token, Func<JToken,string> name, [CanBeNull] string elementType = null)
    {
        _markDirty = markDirty;
        Token = token;
        ElementType = elementType ?? name(token);
        getName = name;
        Classes = new();
        Children = new();
        foreach (var subToken in token)
        {
            if (subToken is not JProperty property) continue;
            Classes.Add(property.Name);
            Children.Add(new JTokenSelectable(markDirty,property.Value,property.Name));
        }
    }

    /// <inheritdoc />
    public sealed override List<ISelectable> Children { get; }

    private Func<JToken, string> getName;

    /// <inheritdoc />
    public override string Name => getName.Invoke(Token);

    /// <inheritdoc />
    public sealed override List<string> Classes { get; }

    /// <inheritdoc />
    public override string ElementType { get; }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) =>
        other is JTokenSelectable jTokenSelectable && jTokenSelectable.Token == Token;

    /// <inheritdoc />
    public override IModifiable OpenModification()
    {
        return new JTokenModifiable(Token,_markDirty);
    }

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        var obj = new JObject();
        if (Token is JArray jArray)
        {
            jArray[elementType] = obj;
        } else if (Token is JObject jObject)
        {
            jObject[elementType] = obj;
        }
        else
        {
            throw new InvalidOperationException();
        }
        var n = new JTokenSelectable(_markDirty, obj, elementType);
        Children.Add(n);
        return n;
    }

    /// <inheritdoc />
    public override string Serialize() => Token.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(Token);
}