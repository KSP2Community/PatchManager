using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that selects children of a selectable that match a selector
/// </summary>
public class ChildSelector : Selector
{
    /// <summary>
    /// The selector to get the parent selectables
    /// </summary>
    public readonly Selector Parent;
    /// <summary>
    /// The selector to match children with
    /// </summary>
    public readonly Selector Child;
    internal ChildSelector(Coordinate c, Selector parent, Selector child) : base(c)
    {
        Parent = parent;
        Child = child;
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectables) =>
        SelectChildren(Parent.SelectAll(selectables));

    private List<SelectableWithEnvironment> SelectChildren(List<SelectableWithEnvironment> selectedParents)
    {
        
        List<SelectableWithEnvironment> result = new();
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var selectable in selectedParents)
        {
            // We are going to get every child of this selectable
            var allChildren = Child.SelectAll(selectable.Selectable.SelectEverything().Select(x => new SelectableWithEnvironment
            {
                Selectable = x,
                Environment = new Execution.Environment(selectable.Environment.GlobalEnvironment, selectable.Environment)
            }).ToList());
            result = SelectionUtilities.CombineSelections(result, allChildren);
        }
        return result;
    }
    
    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAllTopLevel(string type, string name, string data, Environment baseEnvironment, out ISelectable rulesetMatchingObject) =>
        SelectChildren(Parent.SelectAllTopLevel(type, name, data, baseEnvironment, out rulesetMatchingObject));

    public override List<SelectableWithEnvironment> CreateNew(List<DataValue> rulesetArguments, Environment baseEnvironment, out INewAsset newAsset) =>
        SelectChildren(Parent.CreateNew(rulesetArguments, baseEnvironment, out newAsset));
}