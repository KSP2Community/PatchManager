using KSP.Game.Science;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;
using PatchManager.Science.Modifiables;

namespace PatchManager.Science.Selectables;

/// <summary>
/// A selectable that represents discoverables
/// </summary>
public sealed class DiscoverablesSelectable : BaseSelectable
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
    public JObject DiscoverablesObject;
    private JArray _discoverablesArray;
    
    /// <summary>
    /// Create a new discoverables selectable from a JObject
    /// </summary>
    /// <param name="discoverablesObject">Discoverable </param>
    public DiscoverablesSelectable(JObject discoverablesObject)
    {
        DiscoverablesObject = discoverablesObject;
        ElementType = "discoverables";
        Name = discoverablesObject["BodyName"].Value<string>();
        Children = [];
        Classes =
        ["BodyName"];
        _discoverablesArray = (JArray)discoverablesObject["Discoverables"];
        foreach (var jToken in _discoverablesArray)
        {
            var discoverable = (JObject)jToken;
            var name = discoverable["ScienceRegionId"]!.Value<string>();
            Classes.Add(name);
            Children.Add(new JTokenSelectable(SetModified, discoverable,
                disc => ((JObject)disc)["ScienceRegionId"]!.Value<string>(), "discoverable"));
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
        foreach (var jToken in _discoverablesArray)
        {
            var discoverable = (JObject)jToken;
            var name = discoverable["ScienceRegionId"]!.Value<string>();
            if (name != @class) continue;
            classValue = DataValue.FromJToken(discoverable);
            return true;
        }
        classValue = DataValue.Null;
        return false;
    }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) => other is DiscoverablesSelectable discoverablesSelectable &&
                                                        discoverablesSelectable.DiscoverablesObject ==
                                                        DiscoverablesObject;

    /// <inheritdoc />
    public override IModifiable OpenModification() => new DiscoverablesModifiable(this);

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
        _discoverablesArray.Add(jObject);
        var selectable = new JTokenSelectable(SetModified, jObject,
            disc => ((JObject)disc)["ScienceRegionId"]!.Value<string>(), "discoverable");
        Classes.Add(elementType);
        Children.Add(selectable);
        return selectable;
    }

    /// <inheritdoc />
    public override string Serialize() => _deleted ? "" : DiscoverablesObject.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(DiscoverablesObject);

    /// <inheritdoc />
    public override string ElementType { get; }
}