using KSP.Game.Science;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Rulesets;
/// <summary>
/// Ruleset for science regions
/// </summary>
[PatcherRuleset("regions","science_region")]
public class RegionsRuleset : IPatcherRuleSet
{
    /// <inheritdoc />
    public bool Matches(string label) => label == "science_region";

    /// <inheritdoc />
    public ISelectable ConvertToSelectable(string type, string name, string jsonData) =>
        new RegionsSelectable(JObject.Parse(jsonData));

    /// <inheritdoc />
    public INewAsset CreateNew(List<DataValue> dataValues) 
    {
        var bodyName = dataValues[0].String;
        var bakedDiscoverables = new CelestialBodyScienceRegionsData
        {
            BodyName = bodyName,
            SituationData = new CBSituationData(),
            Regions = [],
            Version = dataValues.Count > 1 ? dataValues[1].String : "0.1"
        };
        return new NewGenericAsset("science_region",
            $"{bodyName.ToLowerInvariant()}_science_regions",
            new RegionsSelectable(JObject.FromObject(bakedDiscoverables)));
    }
}