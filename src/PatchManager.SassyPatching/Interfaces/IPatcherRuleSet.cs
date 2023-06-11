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
    public bool Matches(string label);

    /// <summary>
    /// This converts json data to an ISelectable for the rest of the engine to use
    /// </summary>
    /// <param name="jsonData">The data to convert to an ISelectable</param>
    /// <returns>The selectable representing the data</returns>
    public ISelectable ConvertToSelectable(string jsonData);
}