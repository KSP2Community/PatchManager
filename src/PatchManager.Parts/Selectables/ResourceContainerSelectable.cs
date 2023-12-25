using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Parts.Selectables;

/// <summary>
/// A selectable that operates on resource container objects specifically
/// </summary>
public sealed class ResourceContainerSelectable : JTokenSelectable
{
    internal ResourceContainerSelectable(JObject container, PartSelectable selectable)
        : base(selectable.SetModified, container, "resource_container")
    {
        Classes.Add((string)container["name"]);
    }
}