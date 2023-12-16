using KSP.IO;
using KSP.Networking.MP.Utils;
using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Parts.Selectables;

/// <summary>
/// Represents the selectable data in a part module;
/// </summary>
public sealed class ModuleSelectable : BaseSelectable
{
    public readonly JToken SerializedData;
    public readonly PartSelectable Selectable;
    private Dictionary<string, int> _dataIndices;

    /// <inheritdoc />
    public ModuleSelectable(JToken token, PartSelectable selectable)
    {
        SerializedData = token;
        Selectable = selectable;
        _dataIndices = new Dictionary<string, int>();
        ElementType = ((string)token["Name"]).Replace("PartComponent", "");
        Name = ElementType;
        Classes = new();
        Children = new();
        // Now we go down the list in the data type
        var data = (JArray)token["ModuleData"];
        var index = 0;
        foreach (var moduleData in data)
        {
            _dataIndices[moduleData["Name"].Value<string>()] = index++;
            Classes.Add(moduleData["Name"].Value<string>());
            // Where we are going to have to add children ree
            // TODO: Add a specialization for ModuleEngine
            Children.Add(GetSelectable((JObject)moduleData));
        }
    }

    private ISelectable GetSelectable(JObject moduleData)
    {
        var type = Type.GetType(moduleData["DataType"].Value<string>());
        if (type != null && PartsUtilities.ModuleDataAdapters.TryGetValue(type, out var adapterType))
        {
            return (ISelectable)Activator.CreateInstance(adapterType, moduleData, this);
        }
        return new JTokenSelectable(Selectable.SetModified, moduleData["DataObject"], moduleData["Name"].Value<string>());
    }

    /// <inheritdoc />
    public override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override bool MatchesClass(string @class, out DataValue classValue) {
        
        if (_dataIndices.TryGetValue(@class.Replace("PartComponent", ""), out var index))
        {
            classValue = DataValue.FromJToken(SerializedData["ModuleData"][index]["DataObject"]);
            return true;
        }
        classValue = null;
        return false;
    }

    /// <inheritdoc />
    public override string ElementType { get; }
    


    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) =>
        other is ModuleSelectable moduleSelectable && moduleSelectable.SerializedData == SerializedData;

    /// <inheritdoc />
    public override IModifiable OpenModification()
    {
        return new JTokenModifiable(SerializedData, Selectable.SetModified);
    }

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        if (!PartsUtilities.DataModules.TryGetValue(elementType, out var dataModuleType))
        {
            throw new Exception($"Unknown data module {elementType}");
        }
        Selectable.SetModified();
        var instance = (ModuleData)Activator.CreateInstance(dataModuleType);
        var dataObject = new JObject
        {
            ["$type"] = $"{dataModuleType.FullName}, {dataModuleType.Assembly.GetName().Name}"
        };
        var otherObject = JObject.Parse(IOProvider.ToJson(instance));
        foreach (var prop in otherObject)
        {
            dataObject[prop.Key] = prop.Value;
        }
        var trueType = new JObject
        {
            ["Name"] =  dataModuleType.Name,
            ["ModuleType"] = instance.ModuleType.AssemblyQualifiedName,
            ["DataType"] = instance.DataType.AssemblyQualifiedName,
            ["Data"] = null,
            ["DataObject"] = dataObject
        };
        (SerializedData["ModuleData"] as JArray)?.Add(trueType);
        Classes.Add(dataModuleType.Name);
        var selectable = GetSelectable(trueType);
        Children.Add(selectable);
        return selectable;
    }

    /// <inheritdoc />
    public override string Serialize() => SerializedData.ToString();
    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(SerializedData);
}