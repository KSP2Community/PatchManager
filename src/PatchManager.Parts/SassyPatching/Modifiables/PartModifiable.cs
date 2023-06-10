using Newtonsoft.Json.Linq;
using PatchManager.Parts.SassyPatching.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;


namespace PatchManager.Parts.SassyPatching.Modifiables;

/// <summary>
/// Represents the modifiable state of a part_json file
/// </summary>
public sealed class PartModifiable : CustomJTokenModifiable
{
    private PartSelectable _selectable;
    internal PartModifiable(PartSelectable selectable) : base(selectable._jObject["data"],selectable.SetModified)
    {
        _selectable = selectable;
        CustomIndexAdaptors = new();
        CustomElementAdaptors = new();
        CustomClassAdaptors = new()
        {
            ["resourceContainers"] = ResourceContainerClassAdapter,
            ["attachNodes"] = AttachNodesClassAdaptor
        };
    }


    private static bool ResourceContainerClassAdapter(JToken resourceContainer, string className) =>
        resourceContainer.Contains("name") && (string)resourceContainer["name"] == className;

    private static bool AttachNodesClassAdaptor(JToken attachNode, string className) =>
        attachNode.Contains("nodeID") && (string)attachNode["nodeID"] == className;
    
    private static JToken ModuleElementAdaptor(JToken module, string elementName)
    {
        var moduleData = module["ModuleData"];
        return moduleData.FirstOrDefault(data => (string)data["Name"] == elementName);
    }

    private static JToken ModuleIndexAdaptor(JToken module, ulong index)
    {
        return module["ModuleData"][(int)index];
    }

    /// <inheritdoc />
    protected override bool CustomFieldAdaptor(string fieldName, out JToken field, out Func<JToken, string, bool> classAdaptor, out Func<JToken, ulong, JToken> indexAdaptor,
        out Func<JToken, string, JToken> elementAdaptor)
    {
        classAdaptor = null;
        elementAdaptor = null;
        indexAdaptor = null;
        field = null;
        var repl = fieldName.Replace("PartComponent", "");
        foreach (var module in JToken["serializedPartModules"])
        {
            if (((string)module["Name"]).Replace("PartComponent", "") != repl) continue;
            classAdaptor = null; // As modules have a weird childing system, so we can't easily just do this w/ our custom json adapter
            elementAdaptor = ModuleElementAdaptor;
            indexAdaptor = ModuleIndexAdaptor;
            field = module;
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    protected override Dictionary<string, Func<JToken, string, bool>> CustomClassAdaptors { get; }

    /// <inheritdoc />
    protected override Dictionary<string, Func<JToken, ulong, JToken>> CustomIndexAdaptors { get; }

    /// <inheritdoc />
    protected override Dictionary<string, Func<JToken, string, JToken>> CustomElementAdaptors { get; }

    /// <inheritdoc />
    public override void Set(Value value)
    {
        if (value.IsDeletion)
        {
            _selectable.SetDeleted();
        }
        base.Set(value);
    }
}