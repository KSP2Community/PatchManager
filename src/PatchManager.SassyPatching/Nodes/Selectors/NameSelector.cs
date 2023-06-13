using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that matches selectables that have a name that matches a name pattern
/// </summary>
public class NameSelector : Selector
{    
    /// <summary>
    /// The name pattern to match against, can contain */? wildcards
    /// </summary>
    public readonly string NamePattern;
    internal NameSelector(Coordinate c, string namePattern) : base(c)
    {
        NamePattern = namePattern;
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAll(List<ISelectable> selectables)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAllTopLevel(string type, string name, string data, out ISelectable rulesetMatchingObject)
    {
        rulesetMatchingObject = null;
        return new();
    }
}