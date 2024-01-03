using KSP.Game.Science;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;
using PatchManager.Science.Modifiables;

namespace PatchManager.Science.Selectables;

/// <summary>
/// This is the selectable for science regions
/// </summary>
public sealed class RegionsSelectable : BaseSelectable
{
    #pragma warning disable CS0414 // Field is assigned but its value is never used
    private bool _modified;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    private bool _deleted;

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

    
    /// <summary>
    /// The main object that represents this discoverable object
    /// </summary>
    public JObject RegionsObject;
    private JArray _regionsArray;
    
    /// <summary>
    /// Create a new discoverables selectable from a JObject
    /// </summary>
    /// <param name="discoverablesObject">Discoverable </param>
    public RegionsSelectable(JObject regionsObject)
    {
        RegionsObject = regionsObject;
        ElementType = "regions";
        Name = regionsObject["BodyName"].Value<string>();
        Children = [new JTokenSelectable(SetModified, regionsObject["SituationData"], "SituationData","SituationData")];
        Classes = ["BodyName", "SituationData"];
        _regionsArray = (JArray)regionsObject["Regions"];
        foreach (var jToken in _regionsArray)
        {
            var discoverable = (JObject)jToken;
            var name = discoverable["Id"]!.Value<string>();
            Classes.Add(name);
            Children.Add(new JTokenSelectable(SetModified, discoverable,
                disc => ((JObject)disc)["Id"]!.Value<string>(), "region"));
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
        foreach (var jToken in _regionsArray)
        {
            var discoverable = (JObject)jToken;
            var name = discoverable["Id"]!.Value<string>();
            if (name != @class) continue;
            classValue = DataValue.FromJToken(discoverable);
            return true;
        }
        classValue = DataValue.Null;
        return false;
    }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) => other is RegionsSelectable regionsSelectable &&
                                                        regionsSelectable.RegionsObject ==
                                                        RegionsObject;

    /// <inheritdoc />
    public override IModifiable OpenModification() => new RegionsModifiable(this);

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        var position = new CelestialBodyDiscoverablePosition
        {
            ScienceRegionId = elementType,
            Position = new Vector3d(),
            Radius = 0.0
        };
        var jObject = JObject.FromObject(position);
        _regionsArray.Add(jObject);
        var selectable = new JTokenSelectable(SetModified, jObject,
            disc => ((JObject)disc)["ScienceRegionID"]!.Value<string>(), "discoverable");
        Classes.Add(elementType);
        Children.Add(selectable);
        return selectable;
    }

    /// <inheritdoc />
    public override string Serialize() => _deleted ? "" : RegionsObject.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(RegionsObject);

    /// <inheritdoc />
    public override string ElementType { get; }
}