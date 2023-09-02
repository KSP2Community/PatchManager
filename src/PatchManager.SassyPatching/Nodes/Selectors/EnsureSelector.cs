using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Interfaces;

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
    public override List<ISelectable> SelectAll(List<ISelectable> selectables)
    {
        try
        {
            var baseList = selectables.Where(selectable =>
            {
                var result = selectable.MatchesElement(ElementName);
                //Console.WriteLine($"Testing: {asBase?.ElementType} against {ElementName} -> {result}");
                return result;
            }).ToList();
            return baseList.Count == 0 ? selectables.Select(selectable => selectable.AddElement(ElementName)).ToList() : baseList;
        }
        catch (Exception e)
        {
            throw new InterpreterException(Coordinate, e.ToString());
        }
    }

    /// <inheritdoc />
    public override List<ISelectable> SelectAllTopLevel(string type, string name, string data, out ISelectable rulesetMatchingObject)
    {
        rulesetMatchingObject = null;
        return new();
    }

    public override List<ISelectable> CreateNew(List<DataValue> rulesetArguments, out INewAsset newAsset)
    {
        newAsset = null;
        return new();
        
    }
}