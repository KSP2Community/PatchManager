using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.Parts.Selectables;

/// <summary>
/// Represents the resourceContainers field of a part json as a selectable
/// </summary>
public sealed class ResourceContainersSelectable : BaseSelectable
{
    private readonly JArray _containers;
    private PartSelectable _selectable;

    internal ResourceContainersSelectable(JArray containers, PartSelectable selectable)
    {
        _containers = containers;
        Children = new();
        ElementType = "resourceContainers";
        Classes = new();
        Name = "resourceContainers";
        _selectable = selectable;
        foreach (var container in containers)
        {
            Children.Add(new ResourceContainerSelectable(container as JObject, selectable));
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
    public override bool IsSameAs(ISelectable other) =>
        other is ResourceContainersSelectable resourceContainersSelectable &&
        resourceContainersSelectable._containers == _containers;

    /// <inheritdoc />
    public override IModifiable OpenModification() => null;

    /// <inheritdoc />
    public override ISelectable AddElement(string elementType)
    {
        _selectable.SetModified();
        var obj = JObject.FromObject(new
        {
            name = elementType,
            capacityUnits = 0.0,
            initialUnits = 0.0,
            NonStageable = false
        });
        _containers.Add(obj);
        var child = new ResourceContainerSelectable(obj, _selectable);
        Children.Add(child);
        Console.WriteLine($"Added container: {obj}");
        return child;
    }

    /// <inheritdoc />
    public override string Serialize() => _containers.ToString();

    /// <inheritdoc />
    public override DataValue GetValue() => DataValue.FromJToken(_containers);
}