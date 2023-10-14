using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Resources.Selectables;

/// <summary>
/// Represents a selectable for the selection and transformation of recipe data
/// </summary>
public sealed class RecipeSelectable : BaseSelectable
{

    private bool _modified;
    private bool _deleted;
    private readonly string _originalData;
    internal readonly JObject JObject;
    internal readonly JArray Ingredients;

    /// <summary>
    /// Marks this resource selectable as having been modified any level down
    /// </summary>
    public void SetModified()
    {
        _modified = true;
    }

    /// <summary>
    /// Marks this resource as goneso
    /// </summary>
    public void SetDeleted()
    {
        SetModified();
        _deleted = true;
    }
    internal RecipeSelectable(string data)
    {
        _originalData = data;
        JObject = JObject.Parse(data);
        Classes = new() { "recipe" };
        Children = new();
        var resourceData = JObject["data"];
        Name = (string)resourceData["name"];
        Ingredients = (JArray)resourceData["ingredients"];
        ElementType = "recipe";
        foreach (var ingredient in Ingredients)
        {
            Classes.Add(ingredient["name"].Value<string>());
            Children.Add(
                new JTokenSelectable(SetModified, ingredient, tok => tok["name"].Value<string>(), "ingredient"));
        }
    }

    /// <inheritdoc />
    public override List<ISelectable> Children { get; }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override string ElementType { get; }

    /// <inheritdoc />
    public override bool IsSameAs(ISelectable other) => other is RecipeSelectable rs && rs.Name == Name;

    /// <inheritdoc />
    public override IModifiable OpenModification() => new JTokenModifiable(JObject["data"], SetModified);

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        var obj = new JObject
        {
            ["name"] = "Unknown",
            ["unitsPerRecipeUnit"] = 0.00
        };
        var child = new JTokenSelectable(SetModified, obj, tok => tok["name"].Value<string>(), "ingredient");
        Children.Add(child);
        Ingredients.Add(obj);
        return child;
    }

    /// <inheritdoc />
    public override string Serialize() => _modified ? _deleted ? "" : JObject.ToString() : _originalData;

    /// <inheritdoc />
    public override DataValue GetValue() => OpenModification().Get();
}