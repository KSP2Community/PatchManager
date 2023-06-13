using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that matches selectables that don't have a name that matches a name pattern
/// </summary>
public class WithoutNameSelector : Selector
{
    /// <summary>
    /// The name pattern to match against, can contain */? wildcards
    /// </summary>
    public readonly string NamePattern;
    internal WithoutNameSelector(Coordinate c, string namePattern) : base(c)
    {
        NamePattern = namePattern;
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAll(List<ISelectable> selectables)
    {
        return selectables.Where(selectable => !selectable.MatchesName(NamePattern)).ToList();
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAllTopLevel(string type, string name, string data, out ISelectable rulesetMatchingObject)
    {
        rulesetMatchingObject = null;
        return new();
    }
}