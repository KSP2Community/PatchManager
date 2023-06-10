using JetBrains.Annotations;
using PatchManager.SassyPatching.Interfaces;
using Environment = PatchManager.SassyPatching.Execution.Environment;

namespace PatchManager.SassyPatching.Nodes.Selectors;

/// <summary>
/// Represents a selector node
/// </summary>
public abstract class Selector : Node
{
    internal Selector(Coordinate c) : base(c)
    {
    }

    /// <summary>
    /// Select all that match this selector from the list of selectables
    /// </summary>
    /// <param name="selectables">All the selectables to match</param>
    /// <returns>A list of selections</returns>
    public abstract List<ISelectable> SelectAll(List<ISelectable> selectables);

    /// <summary>
    /// Select all that match this selector from the type and data
    /// </summary>
    /// <param name="type">The type, e.g. part_data</param>
    /// <param name="data">The data, a textual representation of the data</param>
    /// <returns>A list of all selections from the data</returns>
    public abstract List<ISelectable> SelectAllTopLevel(string type, string data);

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
    }
}