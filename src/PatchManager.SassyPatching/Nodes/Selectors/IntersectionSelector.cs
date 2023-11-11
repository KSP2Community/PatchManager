using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

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
    public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectableWithEnvironments)
    {
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var selector in Selectors)
        {
            selectableWithEnvironments = SelectionUtilities.IntersectSelections(selectableWithEnvironments, selector.SelectAll(selectableWithEnvironments));
        }
        return selectableWithEnvironments;
    }

    private List<SelectableWithEnvironment> SelectAllSkippingFirst(List<SelectableWithEnvironment> selectables)
    {
        for (var i = 1; i < Selectors.Count; i++)
        {
            selectables = SelectionUtilities.IntersectSelections(selectables, Selectors[i].SelectAll(selectables));
        }
        return selectables;
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAllTopLevel(string type, string name, string data, Environment baseEnvironment, out ISelectable rulesetMatchingObject)
    {
        var start = Selectors[0].SelectAllTopLevel(type, name, data, baseEnvironment, out rulesetMatchingObject);
        return SelectAllSkippingFirst(start);
    }

    public override List<SelectableWithEnvironment> CreateNew(List<DataValue> rulesetArguments, Environment baseEnvironment, out INewAsset newAsset)
    {
        var start = Selectors[0].CreateNew(rulesetArguments,baseEnvironment, out newAsset);
        return SelectAllSkippingFirst(start);
    }
}