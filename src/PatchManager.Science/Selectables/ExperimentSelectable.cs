using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;
using PatchManager.Science.Modifiables;

namespace PatchManager.Science.Selectables;

/// <summary>
/// A selectable that represents a science experiment
/// </summary>
public class ExperimentSelectable : BaseSelectable
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
    /// The science object that this selectable represents
    /// </summary>
    public JObject ScienceObject;

    /// <summary>
    /// The data object that this selectable represents
    /// </summary>
    public JObject DataObject;

    /// <summary>
    /// Creates a new experiment selectable
    /// </summary>
    /// <param name="scienceData">The science data that this selectable represents</param>
    public ExperimentSelectable(JObject scienceData)
    {
        ElementType = "scienceExperiment";
        ScienceObject = scienceData;
        DataObject = (JObject)scienceData["data"]!;
        Classes = [];
        Children = [];
        foreach (var subToken in DataObject)
        {
            Classes.Add(subToken.Key);
            Children.Add(new JTokenSelectable(SetModified,subToken.Value,subToken.Key));
        }
    }

    /// <inheritdoc />
    public sealed override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name => DataObject["ExperimentID"]!.Value<string>()!;

    /// <inheritdoc />
    public sealed override List<string> Classes { get; }

    /// <inheritdoc />
    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        classValue = null;
        if (!MatchesClass(@class))
        {
            return false;
        }

        classValue = DataValue.FromJToken(DataObject[@class]);
        return true;
    }

    /// <inheritdoc />
    public override string ElementType { get; }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) =>
        other is ScienceSelectable selectable && selectable.ScienceObject == ScienceObject;

    /// <inheritdoc />
    public override IModifiable OpenModification() => new ExperimentModifiable(this);

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        var obj = new JObject();
        DataObject[elementType] = obj;
        var n = new JTokenSelectable(SetModified, obj, elementType);
        Children.Add(n);
        return n;
    }

    /// <inheritdoc />
    public override string Serialize() => _deleted ? "" : ScienceObject.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(DataObject);
}