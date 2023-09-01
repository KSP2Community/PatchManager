using System.Text.RegularExpressions;
using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.Modifiables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Parts.Selectables;

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
    internal readonly JObject JObject;
    private static readonly Regex Sanitizer = new("[^a-zA-Z0-9 -_]");
    private static string Sanitize(string str) => Sanitizer.Replace(str, "");

    internal PartSelectable(string data)
    {
        _originalData = data;
        JObject = JObject.Parse(data);
        Classes = new();
        Children = new();
        var partData = JObject["data"];
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
        if (!PartsUtilities.ComponentModules.TryGetValue(elementType, out var mod))
        {
            throw new Exception($"Unknown part module {elementType}");
        }
        SetModified();
        var moduleObject = new JObject()
        {
            ["Name"] = mod.componentModule.Name,
            ["ComponentType"] = mod.componentModule.AssemblyQualifiedName,
            ["BehaviourType"] =  mod.behaviour.AssemblyQualifiedName,
            ["ModuleData"] = new JArray()
        };
        (JObject["data"]["serializedPartModules"] as JArray)?.Add(moduleObject);
        var selectable = new ModuleSelectable(moduleObject, this);
        Classes.Add(elementType.Replace("PartComponent", ""));
        Children.Add(selectable);
        return selectable;
    }


    /// <inheritdoc />
    public override string Serialize() => _modified ? _deleted ? "" : JObject.ToString() : _originalData;

    /// <inheritdoc />
    public override DataValue GetValue() => OpenModification().Get();
}