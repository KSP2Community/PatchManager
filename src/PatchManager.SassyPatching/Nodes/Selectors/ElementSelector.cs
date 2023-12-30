using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that selects all selectables that are an element type
/// </summary>
public class ElementSelector : Selector
{
    /// <summary>
    /// The element type to select
    /// </summary>
    public readonly string ElementName;
    internal ElementSelector(Coordinate c, string elementName) : base(c)
    {
        ElementName = elementName;
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectableWithEnvironments)
    {
        return selectableWithEnvironments.Where(selectable =>
        {
            var interpolated = "";
            try
            {
                interpolated = ElementName.Interpolate(selectable.Environment);
            }
            catch (Exception e)
            {
                throw new InterpolationException(Coordinate, e.Message);
            }

            var result = selectable.Selectable.MatchesElement(interpolated);
            return result;
        }).ToList();
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAllTopLevel(string type, string name, string data, Environment baseEnvironment, out ISelectable rulesetMatchingObject)
    {
        rulesetMatchingObject = null;
        return new();
    }

    public override List<SelectableWithEnvironment> CreateNew(List<DataValue> rulesetArguments, Environment baseEnvironment, out INewAsset newAsset)
    {
        newAsset = null;
        return new();
    }
}