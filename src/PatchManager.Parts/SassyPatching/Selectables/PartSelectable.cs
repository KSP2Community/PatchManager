using System.Text.RegularExpressions;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Parts.SassyPatching.Selectables;

/// <summary>
/// Represents a selectable for selection and transformation of part data
/// </summary>
public sealed class PartSelectable : BaseSelectable
{
    private bool _modified = false;
    private bool _deleted = false;


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

    private readonly string _originalData;
    internal JObject _jObject;
    private static Regex Sanitizer = new Regex("[^a-zA-Z0-9 -_]");
    private string Sanitize(string str)
    {
        return Sanitizer.Replace(str, "");
    }

    internal PartSelectable(string data)
    {
        _originalData = data;
        _jObject = JObject.Parse(data);
        Classes = new();
        Children = new();
        var partData = _jObject["data"];
        Name = (string)partData["partName"];
        foreach (var tag in ((string)partData["tags"]).Split(' '))
        {
            Classes.Add(Sanitize(tag));
        }

        var serializedPartModules = partData["serializedPartModules"];
        foreach (var module in serializedPartModules)
        {
            // var moduleData = module["ModuleData"];
            Classes.Add(((string)module["Name"]).Replace("PartComponent", ""));
            // Classes.Add((string)moduleData["name"]);
            Children.Add(new ModuleSelectable(module,this));
        }
        Children.Add(new ResourceContainersSelectable((JArray)partData["resourceContainers"],this));
        // I'd add more but meh, too much work atm for me to then have to run simple tests, this is the MVP
        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (JProperty child in partData)
        {
            if (child.Name != "resourceContainers")
            {
                Children.Add(new JTokenSelectable(SetModified, child.Value, child.Name));
            }
        }
    }

    /// <inheritdoc />
    public override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override string ElementType => "parts_data";

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) => other is PartSelectable ps && ps.Name == Name;

    /// <inheritdoc />
    public override IModifiable OpenModification()
    {
        return new PartModifiable(this);
    }

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        throw new NotImplementedException("Module addition is not implemented in PatchManager.Core just yet");
    }


    /// <inheritdoc />
    public override string Serialize()
    {
        return !_modified ? _originalData : (!_deleted? _jObject.ToString() : "");
    }
}