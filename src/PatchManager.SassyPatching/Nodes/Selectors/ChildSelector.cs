using PatchManager.SassyPatching.Interfaces;

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
    public override List<ISelectable> SelectAll(List<ISelectable> selectables) =>
        SelectChildren(Parent.SelectAll(selectables));

    private List<ISelectable> SelectChildren(List<ISelectable> selectedParents)
    {
        
        List<ISelectable> result = new();
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var selectable in selectedParents)
        {
            // We are going to get every child of this selectable
            var allChildren = Child.SelectAll(selectable.SelectEverything());
            result = SelectionUtilities.CombineSelections(result, allChildren);
        }
        return result;
    }
    
    /// <inheritdoc />
    public override List<ISelectable> SelectAllTopLevel(string type, string data, out ISelectable rulesetMatchingObject) =>
        SelectChildren(Parent.SelectAllTopLevel(type, data, out rulesetMatchingObject));
}