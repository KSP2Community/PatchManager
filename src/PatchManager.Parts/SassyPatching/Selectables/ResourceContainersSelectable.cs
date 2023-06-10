using Antlr4.Runtime.Misc;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.Parts.SassyPatching.Selectables;

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
        throw new NotImplementedException("Adding resource containers has not yet been implemented");
    }

    /// <inheritdoc />
    public override string Serialize() => _containers.ToString();
}