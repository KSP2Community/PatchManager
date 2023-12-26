namespace PatchManager.SassyPatching.Interfaces;


// Essentially the ruleset requires ISelectable
/// <summary>
/// A ruleset for the patcher (the ":..." selectors)
/// </summary>
public interface IPatcherRuleSet
{
    /// <summary>
    /// What type of patch type will this ruleset match
    /// </summary>
    /// <param name="label">The label to match</param>
    /// <returns>True if the label matches the ruleset</returns>
    public bool Matches(string label);

    /// <summary>
    /// This converts json data to an ISelectable for the rest of the engine to use
    /// </summary>
    /// <param name="type">The type of data to convert to an ISelectale</param>
    /// <param name="name">The name of the data</param>
    /// <param name="jsonData">The data to convert to an ISelectable</param>
    /// <returns>The selectable representing the data</returns>
    public ISelectable ConvertToSelectable(string type, string name, string jsonData);


    /// <summary>
    /// Creates a new asset for the patcher
    /// </summary>
    /// <param name="dataValues">The data values to create the asset from</param>
    /// <returns>The new asset</returns>
    public INewAsset CreateNew(List<DataValue> dataValues);
}