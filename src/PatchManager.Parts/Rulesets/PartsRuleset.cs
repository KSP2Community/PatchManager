using PatchManager.Parts.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;

namespace PatchManager.Parts.Rulesets;

/// <summary>
/// The `:parts` ruleset used by sassy patching
/// </summary>
[PatcherRuleset("parts", "parts_data")]
public class PartsRuleset : IPatcherRuleSet
{
    /// <inheritdoc />
    public bool Matches(string label) => label == "parts_data";

    /// <summary>
    /// Converts the part json to an ISelectable following this ruleset
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name">The name of the file (unused)</param>
    /// <param name="jsonData">The part json</param>
    /// <returns>An ISelectable that follows the part ruleset</returns>
    public ISelectable ConvertToSelectable(string type, string name, string jsonData)
    {
        return new PartSelectable(jsonData);
    }

    /// <inheritdoc />
    public INewAsset CreateNew(List<DataValue> dataValues) => throw new NotImplementedException();
}