namespace PatchManager.Core.SassyPatches;

internal static class SelectionUtilities
{
    public static List<ISelectable> CombineSelections(List<ISelectable> a, List<ISelectable> b)
    {
        var combination = a.ToList();
        combination.AddRange(b.Where(y => combination.Any(x => x.IsSameAs(y))));
        return combination;
    }

    public static List<ISelectable> IntersectSelections(List<ISelectable> a, List<ISelectable> b)
    {
        return a.Where(x => b.Any(x.IsSameAs)).ToList();
    }
}