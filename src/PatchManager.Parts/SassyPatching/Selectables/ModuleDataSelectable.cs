using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.Parts.SassyPatching.Selectables;

public class ModuleDataSelectable : BaseSelectable
{
    public override List<ISelectable> Children { get; }
    public override string Name { get; }
    public override List<string> Classes { get; }
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
        throw new NotImplementedException();
    }

    public override string Serialize()
    {
        throw new NotImplementedException();
    }
}