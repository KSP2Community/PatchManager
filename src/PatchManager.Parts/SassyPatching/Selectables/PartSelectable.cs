using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.Parts.SassyPatching.Selectables;

/// <summary>
/// Represents a selectable for selection and transformation of part data
/// </summary>
public sealed class PartSelectable : BaseSelectable
{
    private bool _modified = true;

    /// <summary>
    /// Marks this part selectable as having been modified any level down
    /// </summary>
    public void SetModified()
    {
        _modified = true;
    }

    private readonly string _originalData;
    private JObject _jObject;

    public PartSelectable(string data)
    {
        _originalData = data;
        _jObject = JObject.Parse(data);
        Classes = new();
        Children = new();
        var partData = _jObject["data"];
        var serializedPartModules = partData["serializedPartModules"];
        foreach (var module in serializedPartModules)
        {
            // var moduleData = module["ModuleData"];
            Classes.Add(((string)module["Name"]).Replace("PartComponent", ""));
            // Classes.Add((string)moduleData["name"]);
            Children.Add(new ModuleSelectable(module));
        }
    }

    /// <inheritdoc />
    public override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override string ElementType => "part";

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) => other is PartSelectable ps && ps.Name == Name;

    public override IModifiable OpenModification()
    {
        _modified = true;

        return new PartModifiable(this);
    }

    public override ISelectable AddElement(string elementType)
    {
        throw new NotImplementedException("Module addition is not implemented in PatchManager.Core just yet");
    }


    /// <inheritdoc />
    public override string Serialize()
    {
        return !_modified ? _originalData : _jObject.ToString();
    }
}