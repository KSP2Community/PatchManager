using Newtonsoft.Json.Linq;
using PatchManager.Resources.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.NewAssets;

namespace PatchManager.Resources.Rulesets;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
/// <summary>
/// A patcher ruleset meant for adapting/creating resource definitions
/// </summary>
[PatcherRuleset("resources", "resources")]
public class ResourceRuleset : IPatcherRuleSet
{
    /// <inheritdoc />
    public bool Matches(string label) => label == "resources";

    /// <inheritdoc />
    public ISelectable ConvertToSelectable(string type, string name, string jsonData) => new ResourceSelectable(jsonData);

    /// <inheritdoc />
    public INewAsset CreateNew(List<DataValue> dataValues)
    {
        var value =
            $"{{\n    \"version\": 0.1,\n    \"useExternal\": false,\n    \"data\": {{\n        \"name\": \"{dataValues[0].String}\",\n        \"displayNameKey\": \"Resource/DisplayName/Unknown\",\n        \"abbreviationKey\": \"Resource/Abbreviation/UK\",\n        \"isTweakable\": true,\n        \"isVisible\": true,\n        \"massPerUnit\": 0,\n        \"volumePerUnit\": 0,\n        \"specificHeatCapacityPerUnit\": 0,\n        \"flowMode\": 0,\n        \"transferMode\": 0,\n        \"costPerUnit\": 0,\n\t\"NonStageable\": false,\n        \"resourceIconAssetAddress\": \"\",\n        \"vfxFuelType\": \"NoFuel\"     \n    }}    \n}}";
        return new NewGenericAsset("resources", dataValues[0].String, new ResourceSelectable(value));
    }
}