using JetBrains.Annotations;
using KSP.IO;
using KSP.Modules;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.Attributes;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Parts.Selectables;

/// <summary>
/// Represents a selectable for the selection and transformation of Data_Engine
/// </summary>
[ModuleDataAdapter(typeof(Data_Engine))]
[UsedImplicitly]
public sealed class DataEngineSelectable : BaseSelectable
{
    /// <summary>
    /// The serialized data for this selectable
    /// </summary>
    public readonly JObject SerializedData;

    /// <summary>
    /// The part selectable that owns this selectable
    /// </summary>
    public readonly PartSelectable Selectable;

    /// <summary>
    /// Initialize the selectable
    /// </summary>
    /// <param name="moduleData">Module data</param>
    /// <param name="moduleSelectable">Module selectable</param>
    public DataEngineSelectable(JObject moduleData, ModuleSelectable moduleSelectable)
    {
        SerializedData = moduleData;
        Name = "Data_Engine";
        Selectable = moduleSelectable.Selectable;
        ElementType = moduleData["Name"].Value<string>();
        Classes = new();
        Children = new();
        foreach (var field in moduleData)
        {
            Classes.Add(field.Key);
            if (field.Value.Type == JTokenType.Object)
            {
                Children.Add(new JTokenSelectable(Selectable.SetModified, field.Value, field.Key, field.Key));
            }
        }
        foreach (var jToken in (JArray)moduleData["engineModes"])
        {
            var mode = (JObject)jToken;
            Classes.Add(mode["engineID"].Value<string>());
            Children.Add(new JTokenSelectable(Selectable.SetModified,mode,m => m["engineID"].Value<string>(),"engine_mode"));
        }
    }

    /// <inheritdoc />
    public override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        if (SerializedData.TryGetValue(@class, out var value))
        {
            classValue = DataValue.FromJToken(value);
            return true;
        }

        foreach (var jToken in (JArray)SerializedData["engineModes"])
        {
            var mode = (JObject)jToken;
            if (mode["engineID"].Value<string>() != @class)
            {
                continue;
            }

            classValue = DataValue.FromJToken(mode);
            return true;
        }

        classValue = DataValue.Null;
        return false;
    }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) =>
        (other is DataEngineSelectable dataEngineSelectable) &&
        SerializedData == dataEngineSelectable.SerializedData;

    /// <inheritdoc />
    public override IModifiable OpenModification() => new JTokenModifiable(SerializedData, Selectable.SetModified);

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        var engineModeData = new Data_Engine.EngineMode()
        {
            engineID = elementType
        };
        var json = JObject.FromObject(engineModeData);
        ((JArray)SerializedData["engineModes"]).Add(json);
        return new JTokenSelectable(Selectable.SetModified, json, mode => mode["engineID"].Value<string>(),
            "engine_mode");
    }

    /// <inheritdoc />
    public override string Serialize() => SerializedData.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(SerializedData);

    /// <inheritdoc />
    public override string ElementType { get; }
}