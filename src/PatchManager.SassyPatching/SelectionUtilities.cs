using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching;

internal static class SelectionUtilities
{
    public static List<SelectableWithEnvironment> CombineSelections(List<SelectableWithEnvironment> a, List<SelectableWithEnvironment> b)
    {
        var combination = a.ToList();
        combination.AddRange(b.Where(y => !combination.Any(x => x.Selectable.IsSameAs(y.Selectable))));
        return combination;
    }

    public static List<SelectableWithEnvironment> IntersectSelections(List<SelectableWithEnvironment> a, List<SelectableWithEnvironment> b)
    {
        return a.Where(x => b.Any(y => x.Selectable.IsSameAs(y.Selectable))).ToList();
    }

}