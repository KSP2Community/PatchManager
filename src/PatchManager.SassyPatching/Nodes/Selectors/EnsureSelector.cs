using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that adds an element to a selectable and selects it
/// </summary>
public class EnsureSelector : Selector
{
    /// <summary>
    /// The element type to add to the selectable
    /// </summary>
    public string ElementName;
    internal EnsureSelector(Coordinate c, string elementName) : base(c)
    {
        ElementName = elementName;
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectableWithEnvironments)
    {
        try
        {
            var baseList = selectableWithEnvironments.Where(selectable =>
            {
                var result = selectable.Selectable.MatchesElement(ElementName);
                return result;
            }).ToList();
            // return baseList.Count == 0 ? selectables.Select(selectable => selectable.AddElement(ElementName)).ToList() : baseList;
            if (baseList.Count == 0)
            {
                foreach (var selectable in selectableWithEnvironments)
                {
                    var addedElement = selectable.Selectable.AddElement(ElementName);
                    baseList.Add(new SelectableWithEnvironment
                    {
                        Selectable = addedElement,
                        Environment = new Environment(selectable.Environment.GlobalEnvironment,selectable.Environment)
                    });
                }
            }
            return baseList;
        }
        catch (Exception e)
        {
            throw new InterpreterException(Coordinate, e.ToString());
        }
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