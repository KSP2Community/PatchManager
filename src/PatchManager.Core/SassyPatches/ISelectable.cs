namespace PatchManager.Core.SassyPatches;

public interface ISelectable
{
    public List<ISelectable> SelectByName(string name);
    public List<ISelectable> SelectByClass(string @class);
    public List<ISelectable> SelectWithoutClass(string @class);
    public List<ISelectable> SelectByElement(string element);
    public List<ISelectable> SelectEverything();
    public bool IsSameAs(ISelectable other);
}