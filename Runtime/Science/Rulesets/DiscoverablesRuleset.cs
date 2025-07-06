using System;
using System.Collections.Generic;
using KSP.Game.Science;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.Science.Selectables;

namespace PatchManager.Science.Rulesets
{
    /// <summary>
    /// A ruleset for modifying discoverable positions in the game
    /// </summary>
    [PatcherRuleset("discoverables","science_region_discoverables")]
    public class DiscoverablesRuleset : IPatcherRuleSet
    {

        public string[] Labels => new[] { "science_region_discoverables" };

        /// <inheritdoc />
        public ISelectable ConvertToSelectable(string type, string name, string jsonData) =>
            new DiscoverablesSelectable(JObject.Parse(jsonData));

        public bool CanIngestSelectable(ISelectable selectable) => selectable is DiscoverablesSelectable
        {
            Deleted: false
        };

        // TODO: Evaluate whether or not this is the case
        public bool CanGetAssetNameFromSelectableName => false;
        public string SelectableNameToAssetName(string selectableName)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public INewAsset CreateNew(List<DataValue> dataValues)
        {
            var bodyName = dataValues[0].String;
            var bakedDiscoverables = new CelestialBodyBakedDiscoverables
            {
                BodyName = bodyName,
                Discoverables = Array.Empty<CelestialBodyDiscoverablePosition>(),
                Version = dataValues.Count > 1 ? dataValues[1].String : "0.1"
            };
            return new NewGenericAsset("science_region_discoverables",
                $"{bodyName.ToLowerInvariant()}_science_regions_discoverables",
                new DiscoverablesSelectable(JObject.FromObject(bakedDiscoverables)));
        }
    }
}