using PatchManager.SassyPatching.Execution;
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
    public abstract List<SelectableWithEnvironment> SelectAll(List<SelectableWithEnvironment> selectables);

    /// <summary>
    /// Select all that match this selector from the type and data
    /// </summary>
    /// <param name="type">The type, e.g. parts_data</param>
    /// <param name="name">The name of the data</param>
    /// <param name="data">The data, a textual representation of the data</param>
    /// <param name="baseEnvironment">The base environment to create the selectables in</param>
    /// <param name="rulesetMatchingObject">The found object that matches the ruleset</param>
    /// <returns>A list of all selections from the data</returns>
    public abstract List<SelectableWithEnvironment> SelectAllTopLevel(string type, string name, string data, Environment baseEnvironment, out ISelectable rulesetMatchingObject);


    /// <summary>
    /// Create a new asset (flows up until the ruleset statement)
    /// </summary>
    /// <param name="rulesetArguments">The arguments to the ruleset</param>
    /// <param name="newAsset">The reference to the newly created asset</param>
    /// <param name="baseEnvironment">The base environment to create the selectables in</param>
    /// <returns>The new assets selectables</returns>
    public abstract List<SelectableWithEnvironment> CreateNew(List<DataValue> rulesetArguments, Environment baseEnvironment, out INewAsset newAsset);

    /// <inheritdoc />
    public override void ExecuteIn(Environment environment)
    {
    }
}