using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using PatchManager.Planets.Overrides;
using PatchManager.Planets.Selectables;

namespace PatchManager.Planets.Rulesets
{
    [PatcherRuleset("atmosphere-override","atmosphere_overrides")]
    public class AtmosphereOverrideRuleset : IPatcherRuleSet
    {
        public bool Matches(string label) => label == "atmosphere_overrides";

        public ISelectable ConvertToSelectable(string type, string name, string jsonData) =>
            new AtmosphereOverrideSelectable(JObject.Parse(jsonData));

        public INewAsset CreateNew(List<DataValue> dataValues)
        {
            var planetName = dataValues[0].String.ToLowerInvariant();
            var data = new AtmosphereOverride
            {
                PlanetName = planetName
            };
            return new NewGenericAsset("atmosphere_overrides", $"atmosphere_override_{planetName}",
                new AtmosphereOverrideSelectable(JObject.FromObject(data)));
        }
    }
}
