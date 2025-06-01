using System.Collections.Generic;
using KSP.Sim.Definitions;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using Shoemaker.Selectables;

namespace Shoemaker.Rulesets
{
    [PatcherRuleset("body","celestial_bodies")]
    public class CelestialBodyRuleset : IPatcherRuleSet
    {
        public bool Matches(string label) => label == "celestial_bodies";

        public ISelectable ConvertToSelectable(string type, string name, string jsonData) =>
            new CelestialBodySelectable(JObject.Parse(jsonData));

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
            return new NewGenericAsset("celestial_bodies", "bodyName",
                new CelestialBodySelectable(JObject.FromObject(core)));
        }
    }
}