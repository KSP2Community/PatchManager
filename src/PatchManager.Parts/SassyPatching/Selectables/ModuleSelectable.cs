using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.Parts.SassyPatching.Selectables;

public sealed class ModuleSelectable : BaseSelectable
{
    
    public ModuleSelectable(JToken token)
    {
        ElementType = ((string)token["Name"]).Replace("PartComponent", "");
        Classes = new();
        // Now we go down the list in the data type
        var data = token["ModuleData"];
        foreach (var moduleData in data)
        {
            Classes.Add((string)moduleData["Name"]);
            // Where we are going to have to add children ree
        }
    }
    
    public override List<ISelectable> Children { get; }
    public override string Name { get; }
    public override List<string> Classes { get; }

    /// <inheritdoc />
    public override string ElementType { get; }
    public override bool IsSameAs(ISelectable other)
    {
        throw new NotImplementedException();
    }

    public override IModifiable OpenModification()
    {
        throw new NotImplementedException();
    }

    public override ISelectable AddElement(string elementType)
    {
        throw new NotImplementedException("Module data addition is not implemented in PatchManager just yet");
    }

    public override string Serialize()
    {
        throw new NotImplementedException();
    }
}