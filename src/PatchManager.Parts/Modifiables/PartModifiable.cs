using Newtonsoft.Json.Linq;
using PatchManager.Parts.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using Enumerable = UniLinq.Enumerable;

namespace PatchManager.Parts.Modifiables;

/// <summary>
/// Represents the modifiable state of a part_json file
/// </summary>
public sealed class PartModifiable : CustomJTokenModifiable
{
    private PartSelectable _selectable;
    internal PartModifiable(PartSelectable selectable) : base(selectable.JObject["data"],selectable.SetModified)
    {
        _selectable = selectable;
    }

    /// <inheritdoc />
    public override DataValue GetFieldValue(string fieldName)
    {
        var repl = fieldName.Replace("PartComponent", "");
        if (JToken is not JObject jObject)
            return DataValue.Null;
        if (!jObject.ContainsKey("serializedPartModules"))
            return DataValue.Null;
        foreach (var module in jObject["serializedPartModules"])
        {
            if (module is not JObject moduleObject) continue;
            if (moduleObject.ContainsKey("Name") && ((string)moduleObject["Name"])!.Replace("PartComponent", "") == repl)
                return DataValue.FromJToken(module);
        }

        if (jObject.TryGetValue(fieldName, out var value))
            return DataValue.FromJToken(value);

        return DataValue.Null;
    }

    /// <inheritdoc />
    public override void Set(DataValue dataValue)
    {
        if (dataValue.IsDeletion)
        {
            _selectable.SetDeleted();
        }
        base.Set(dataValue);
    }
}