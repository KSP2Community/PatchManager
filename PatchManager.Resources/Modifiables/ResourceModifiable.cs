using Newtonsoft.Json.Linq;
using PatchManager.Resources.Selectables;
using PatchManager.SassyPatching.Modifiables;

namespace PatchManager.Resources.Modifiables;

/// <summary>
/// Represents the modifiable state of a resource file
/// </summary>
public class ResourceModifiable : JTokenModifiable
{
    public ResourceModifiable(ResourceSelectable selectable) : base(selectable.JObject["data"], selectable.SetModified) { }
}