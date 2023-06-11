using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;

namespace PatchManager.Parts.SassyPatching.Selectables;

public sealed class ResourceContainerSelectable : JTokenSelectable
{
    private PartSelectable _selectable;

    internal ResourceContainerSelectable(JObject container, PartSelectable selectable) : base(selectable.SetModified,container,"resource_container")
    {
        _selectable = selectable;
        Classes.Add((string)container["name"]);
    }
}