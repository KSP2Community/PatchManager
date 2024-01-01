using PatchManager.SassyPatching.Exceptions;
using PatchManager.SassyPatching.Execution;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Selectors;

public class ClassCaptureSelector : Selector
{
    public readonly string ClassName;
    public List<Node> Captures;
    internal ClassCaptureSelector(Coordinate c, string className, List<Node> captures) : base(c)
    {
        ClassName = className;
        Captures = captures;
    }

    /// <inheritdoc />
    public override List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectableWithEnvironments)
    {
        List<SelectableWithEnvironment> newList = new();
        foreach (var selectableWithEnvironment in selectableWithEnvironments)
        {
            var interpolated = "";
            try
            {
                interpolated = ClassName.Interpolate(selectableWithEnvironment.Environment);
            }
            catch (Exception e)
            {
                throw new InterpolationException(Coordinate, e.Message);
            }

            if (selectableWithEnvironment.Selectable.MatchesClass(interpolated,out var value))
            {
                selectableWithEnvironment.Environment["current"] = value;
                foreach (var node in Captures)
                {
                    node.ExecuteIn(selectableWithEnvironment.Environment);
                }
                selectableWithEnvironment.Environment.ScopedValues.Remove("current");
                newList.Add(selectableWithEnvironment);
            }
        }
        return newList;
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