using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Parts.Selectables;

/// <summary>
/// A selectable that operates on resource container objects specifically
/// </summary>
public sealed class ResourceContainerSelectable : JTokenSelectable
{
    private PartSelectable _selectable;

    internal ResourceContainerSelectable(JObject container, PartSelectable selectable) : base(selectable.SetModified,container,"resource_container")
    {
        _selectable = selectable;
        Classes.Add((string)container["name"]);
    }
}