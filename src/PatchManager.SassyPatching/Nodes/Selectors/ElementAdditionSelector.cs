using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector that adds an element to a selectable and selects it
/// </summary>
public class ElementAdditionSelector : Selector
{
    /// <summary>
    /// The element type to add to the selectable
    /// </summary>
    public string ElementName;
    internal ElementAdditionSelector(Coordinate c, string elementName) : base(c)
    {
        ElementName = elementName;
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectableWithEnvironments)
    {
        try
        {
            List<SelectableWithEnvironment> result = new();
            foreach (var selectable in selectableWithEnvironments)
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
                var addedElement = selectable.Selectable.AddElement(interpolated);
                result.Add(new SelectableWithEnvironment
                {
                    Selectable = addedElement,
                    Environment = new Environment(selectable.Environment.GlobalEnvironment,selectable.Environment)
                });
            }

            return result;
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