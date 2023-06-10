using Antlr4.Runtime.Misc;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Parts.SassyPatching.Selectables;

/// <summary>
/// Represents the selectable data in a part module;
/// </summary>
public sealed class ModuleSelectable : BaseSelectable
{
    private JToken _jToken;
    private PartSelectable _selectable;

    /// <inheritdoc />
    public ModuleSelectable(JToken token, PartSelectable selectable)
    {
        _jToken = token;
        _selectable = selectable;
        ElementType = ((string)token["Name"]).Replace("PartComponent", "");
        Name = ElementType;
        Classes = new();
        Children = new();
        // Now we go down the list in the data type
        var data = token["ModuleData"];
        foreach (var moduleData in data)
        {
            Classes.Add((string)moduleData["Name"]);
            // Where we are going to have to add children ree
            Children.Add(new JTokenSelectable(selectable.SetModified, moduleData["DataObject"], (string)moduleData["Name"]));
        }
    }

    /// <inheritdoc />
    public override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override string ElementType { get; }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) =>
        other is ModuleSelectable moduleSelectable && moduleSelectable._jToken == _jToken;

    /// <inheritdoc />
    public override IModifiable OpenModification()
    {
        return new JTokenModifiable(_jToken, _selectable.SetModified);
    }

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        throw new NotImplementedException("Module data addition is not implemented in PatchManager just yet");
    }

    /// <inheritdoc />
    public override string Serialize() => _jToken.ToString();
}