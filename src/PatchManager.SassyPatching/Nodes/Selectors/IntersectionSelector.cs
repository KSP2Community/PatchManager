using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that selects all selectables that get selected by all of the selectors contained within
/// </summary>
public class IntersectionSelector : Selector
{
    /// <summary>
    /// The list of selectors to get the intersection selection of
    /// </summary>
    public readonly List<Selector> Selectors;
    internal IntersectionSelector(Coordinate c, Selector lhs, Selector rhs) : base(c)
    {
        Selectors = new();
        if (lhs is IntersectionSelector lis)
        {
            Selectors.AddRange(lis.Selectors);
        }
        else
        {
            Selectors.Add(lhs);
        }

        if (rhs is IntersectionSelector ris)
        {
            Selectors.AddRange(ris.Selectors);
        }
        else
        {
            Selectors.Add(rhs);
        }
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAll(List<ISelectable> selectables)
    {
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var selector in Selectors)
        {
            selectables = SelectionUtilities.IntersectSelections(selectables, selector.SelectAll(selectables));
        }
        return selectables;
    }

    private List<ISelectable> SelectAllSkippingFirst(List<ISelectable> selectables)
    {
        for (var i = 1; i < Selectors.Count; i++)
        {
            selectables = SelectionUtilities.IntersectSelections(selectables, Selectors[i].SelectAll(selectables));
        }
        return selectables;
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAllTopLevel(string type, string name, string data, out ISelectable rulesetMatchingObject)
    {
        var start = Selectors[0].SelectAllTopLevel(type, name, data, out rulesetMatchingObject);
        return SelectAllSkippingFirst(start);
    }
}