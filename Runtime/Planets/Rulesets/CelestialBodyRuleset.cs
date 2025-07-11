using System.Collections.Generic;
using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.Planets.Selectables;

namespace PatchManager.Planets.Rulesets
{
    [PatcherRuleset("body", "celestial_bodies")]
    public class CelestialBodyRuleset : IPatcherRuleSet
    {
        public string[] Labels => new[] { "celestial_bodies" };

        public ISelectable ConvertToSelectable(string type, string name, string jsonData) =>
            new CelestialBodySelectable(JObject.Parse(jsonData));

        public bool CanIngestSelectable(ISelectable selectable) => selectable is CelestialBodySelectable
        {
            Deleted: false
        };

        public bool CanGetAssetNameFromSelectableName => true;

        public string[] SelectableNameToAssetName(string selectableName) => new[]
            { selectableName };

        public INewAsset CreateNew(List<DataValue> dataValues)
        {
            var bodyName = dataValues[0].String;
            var core = new CelestialBodyCore
            {
                version = CelestialBodyCore.CELESTIAL_BODY_SERIALIZATION_VERSION,
                data = new CelestialBodyData
                {
                    bodyName = bodyName
                }
            };
            return new NewGenericAsset("celestial_bodies", bodyName,
                new CelestialBodySelectable(JObject.FromObject(core)));
        }
    }
}