using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;
using PatchManager.Science.Modifiables;

namespace PatchManager.Science.Selectables;

public sealed class ScienceSelectable : BaseSelectable
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

    public JObject ScienceObject;
    
    public ScienceSelectable(JObject scienceData)
    {
        ElementType = "techNodeData";
        ScienceObject = scienceData;
        Classes = new();
        Children = new();
        foreach (var subToken in ScienceObject)
        {
            Classes.Add(subToken.Key);
            Children.Add(new JTokenSelectable(SetModified,subToken.Value,subToken.Key));
        }
    }
    /// <inheritdoc />
    public sealed override List<ISelectable> Children { get; }


    /// <inheritdoc />
    public override string Name => ScienceObject["ID"]!.Value<string>()!;

    /// <inheritdoc />
    public sealed override List<string> Classes { get; }

    public override bool MatchesClass(string @class, out DataValue classValue)
    {
        classValue = null;
        if (!MatchesClass(@class))
        {
            return false;
        }

        classValue = DataValue.FromJToken(ScienceObject[@class]);
        return true;

    }

    /// <inheritdoc />
    public override string ElementType { get; }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) =>
        other is ScienceSelectable selectable && selectable.ScienceObject == ScienceObject;

    /// <inheritdoc />
    public override IModifiable OpenModification() => new ScienceModifiable(this);

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        var obj = new JObject();
        ScienceObject[elementType] = obj;
        var n = new JTokenSelectable(SetModified, obj, elementType);
        Children.Add(n);
        return n;
    }

    /// <inheritdoc />
    public override string Serialize() => _deleted ? "" : ScienceObject.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(ScienceObject);
}